using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CRISP.ServiceDefaults.Middlwares
{
    public sealed class ValidationEndpointFilter : IEndpointFilter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ValidationEndpointFilter(IServiceProvider serviceProvider, ILogger<ValidationEndpointFilter> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            _logger.LogTrace("Validating '{url}' that has {n} arguments.", context.HttpContext.Request.GetDisplayUrl(), context.Arguments.Count);
            for (int i = 0; i < context.Arguments.Count; i++)
            {
                object? argument = context.Arguments[i];
                if (argument == null)
                {
                    _logger.LogDebug("The argument {i} was null. Skipping validation.", i);
                    continue;
                }

                Type validatorGenericType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                var validators = _serviceProvider.GetServices(validatorGenericType) as IEnumerable<IValidator>;
                if (validators is null)
                {
                    _logger.LogDebug("No validator found for argument {i}.", i);
                    continue;
                }

                Type contextGenericType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
                var validationContext = Activator.CreateInstance(contextGenericType, argument) as IValidationContext;
                ValidationResult[] results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(validationContext, context.HttpContext.RequestAborted)));
                if (!results.All(r => r.IsValid))
                {
                    var failures = results.Where(r => !r.IsValid).SelectMany(r => r.Errors).ToList();
                    _logger.LogInformation("The validator of argument {i} found {n} errors.", i, failures.Count);
                    throw new ValidationException(failures.Select(e => new ValidationFailure(e.PropertyName, e.ErrorMessage)));
                }
            }

            return await next(context);
        }
    }
}
