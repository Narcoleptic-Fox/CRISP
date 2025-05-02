using CRISP.Options;
using CRISP.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CRISP.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests using registered validators.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;
    private readonly ValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The validation options.</param>
    public ValidationBehavior(
        IServiceProvider serviceProvider,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger,
        ValidationOptions options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public async ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get all validators for the request type
        List<IValidator<TRequest>> validators = _serviceProvider.GetServices<IValidator<TRequest>>().ToList();

        if (validators.Any())
        {
            // Perform validation
            List<ValidationError> errors = [];

            foreach (IValidator<TRequest>? validator in validators)
            {
                ValidationResult validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                    errors.AddRange(validationResult.Errors);
            }

            // If child object validation is enabled, validate child objects recursively
            if (_options.ValidateChildObjects && errors.Count == 0)
                await ValidateChildObjectsRecursively(request, errors, 0, cancellationToken);

            // If there are validation errors, handle them according to options
            if (errors.Any())
            {
                StringBuilder errorMessageBuilder = new();
                errorMessageBuilder.AppendLine("Validation failed:");

                foreach (ValidationError error in errors)
                {
                    if (!string.IsNullOrEmpty(error.PropertyName))
                        errorMessageBuilder.AppendLine($"- {error.PropertyName}: {error.ErrorMessage}");
                    else
                    {
                        errorMessageBuilder.AppendLine($"- {error.ErrorMessage}");
                    }
                }

                string errorMessage = errorMessageBuilder.ToString();

                // Log validation failures if enabled
                if (_options.LogValidationFailures)
                {
                    _logger.LogWarning("Validation failed for request {RequestType}: {ValidationErrors}",
                        typeof(TRequest).Name, errorMessage);
                }

                // Throw exception if configured to do so
                if (_options.ThrowExceptionOnValidationFailure)
                    throw new ValidationException(errorMessage, errors);
            }
        }
        else if (!_options.SkipValidationIfNoValidatorsRegistered)
        {
            // If no validators are registered and the option is set to not skip validation,
            // throw an exception
            throw new InvalidOperationException($"No validators registered for {typeof(TRequest).Name}");
        }

        // If validation passes or there are no validators, proceed with the request
        return await next(cancellationToken);
    }

    private async ValueTask ValidateChildObjectsRecursively(
        object obj,
        List<ValidationError> errors,
        int depth,
        CancellationToken cancellationToken)
    {
        // Prevent stack overflow from circular references
        if (depth >= _options.MaxChildValidationDepth)
            return;

        Type objType = obj.GetType();
        IEnumerable<System.Reflection.PropertyInfo> properties = objType.GetProperties()
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        foreach (System.Reflection.PropertyInfo? property in properties)
        {
            object? value = property.GetValue(obj);

            // Skip null values
            if (value == null)
                continue;

            // Validate collections
            if (value is IEnumerable<object> collection)
            {
                foreach (object item in collection)
                {
                    if (item != null)
                        await ValidateObject(item, property.Name, errors, depth + 1, cancellationToken);
                }
            }
            // Validate objects
            else if (!property.PropertyType.IsPrimitive
                    && property.PropertyType != typeof(string)
                    && property.PropertyType != typeof(DateTime)
                    && property.PropertyType != typeof(DateTimeOffset)
                    && property.PropertyType != typeof(Guid)
                    && property.PropertyType != typeof(decimal)
                    && !property.PropertyType.IsEnum)
            {
                await ValidateObject(value, property.Name, errors, depth + 1, cancellationToken);
            }
        }
    }

    private async ValueTask ValidateObject(
        object obj,
        string propertyPath,
        List<ValidationError> errors,
        int depth,
        CancellationToken cancellationToken)
    {
        Type objType = obj.GetType();
        Type validatorType = typeof(IValidator<>).MakeGenericType(objType);

        // Get validators for this object type
        List<object?> validators = _serviceProvider.GetServices(validatorType).ToList();

        foreach (object? validator in validators)
        {
            System.Reflection.MethodInfo? validateMethod = validatorType.GetMethod(nameof(IValidator<object>.Validate));

            if (validateMethod != null)
            {
                object? result = validateMethod.Invoke(validator, new[] { obj });

                if (result is ValidationResult validationResult && !validationResult.IsValid)
                {
                    // Add property path prefix to validation errors
                    foreach (ValidationError error in validationResult.Errors)
                    {
                        string prefixedPropertyName = string.IsNullOrEmpty(error.PropertyName)
                            ? propertyPath
                            : $"{propertyPath}.{error.PropertyName}";

                        errors.Add(new ValidationError(
                            prefixedPropertyName,
                            error.ErrorMessage));
                    }
                }
            }
        }

        // Validate child objects recursively
        await ValidateChildObjectsRecursively(obj, errors, depth, cancellationToken);
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errors">The validation errors.</param>
    public ValidationException(string message, IEnumerable<ValidationError> errors)
        : base(message) => Errors = errors.ToList();
}