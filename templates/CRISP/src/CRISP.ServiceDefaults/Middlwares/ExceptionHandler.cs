using CRISP.Core.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace CRISP.ServiceDefaults.Middlwares
{
    public sealed class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandler(IWebHostEnvironment environment, ILogger<ExceptionHandler> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, exception.Message);
            _logger.LogError(exception.StackTrace);

            IResult result;
            switch (exception)
            {
                case NotFoundException notFoundException:
                case KeyNotFoundException keyNotFoundException:
                    result = TypedResults.NotFound(exception.Message);
                    break;
                case ArgumentNullException argumentNullException:
                case InvalidEnumArgumentException invalidEnumArgumentException:
                case ArgumentOutOfRangeException argumentOutOfRangeException:
                case ArgumentException argumentException:
                case NotImplementedException notImplementedException:
                case ValidationException validationException:
                case DomainException domainException:
                    result = TypedResults.BadRequest(exception.Message);
                    break;
                case UnauthorizedAccessException unauthorizedAccessException:
                    result = TypedResults.Unauthorized();
                    break;
                default:
                    {
                        string message = "Sorry an error occurred, please try again.";
                        if (!_environment.IsProduction())
                            message = exception.Message;
                        result = TypedResults.InternalServerError(message);
                    }
                    break;
            }

            await result.ExecuteAsync(httpContext);
            return true;
        }
    }
}
