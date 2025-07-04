using Crisp.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Crisp.Middleware;

/// <summary>
/// Middleware that handles exceptions in a procedural, predictable way.
/// Maps domain exceptions to appropriate HTTP status codes and problem details.
/// </summary>
public class CrispExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CrispExceptionMiddleware> _logger;
    private readonly CrispOptions _options;
    private readonly IHostEnvironment _environment;
    /// <summary>
    /// Initializes a new instance of the <see cref="CrispExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance for logging exceptions.</param>
    /// <param name="options">CRISP configuration options.</param>
    public CrispExceptionMiddleware(
        RequestDelegate next,
        ILogger<CrispExceptionMiddleware> logger,
        CrispOptions options,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _options = options;
        _environment = environment;
    }

    /// <summary>
    /// Invokes the middleware to process the HTTP request and handle any exceptions.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task that represents the completion of request processing.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        try
        {
            // Continue down the pipeline
            await _next(context);
        }
        catch (Exception exception)
        {
            // Handle the exception procedurally
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        string correlationId = Guid.NewGuid().ToString();

        // Step 1: Log the exception with context
        LogException(exception, context, correlationId);

        // Step 2: Map exception to status code and response
        ProblemDetails problemDetails = CreateProblemDetails(exception, context, correlationId);

        // Step 3: Write the response
        context.Response.StatusCode = problemDetails.Status!.Value;
        context.Response.ContentType = "application/problem+json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        string json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private ProblemDetails CreateProblemDetails(Exception exception, HttpContext context, string correlationId)
    {
        (int statusCode, string title, string detail) = exception switch
        {
            ValidationException validationEx => (400, "Validation Error", validationEx.Message),
            NotFoundException notFoundEx => (404, "Not Found", notFoundEx.Message),
            UnauthorizedException unauthorizedEx => (401, "Unauthorized", unauthorizedEx.Message),
            ConflictException conflictEx => (409, "Conflict", conflictEx.Message),
            ForbiddenException forbiddenEx => (403, "Forbidden", forbiddenEx.Message),
            CrispException crispEx => (400, "Domain Error", crispEx.Message),
            JsonException jsonEx => (400, "Invalid JSON", "Invalid JSON format in request body"),
            OperationCanceledException => (499, "Request Cancelled", "The operation was cancelled"),
            TimeoutException => (408, "Request Timeout", "The operation timed out"),
            ArgumentException argEx => (400, "Invalid Argument", argEx.Message),
            _ => (500, "Internal Server Error", GetSafeErrorMessage(exception))
        };

        ProblemDetails problemDetails = new()
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

        // Add request information
        problemDetails.Extensions["method"] = context.Request.Method;
        problemDetails.Extensions["path"] = context.Request.Path.Value;

        return problemDetails;
    }

    private string GetSafeErrorMessage(Exception exception) => _environment.IsDevelopment()
            ? exception.Message
            : "An internal error occurred. Please contact support if the problem persists.";

    private void LogException(Exception exception, HttpContext context, string correlationId)
    {
        // Create structured log data
        var logData = new
        {
            CorrelationId = correlationId,
            RequestPath = context.Request.Path.Value,
            context.Request.Method,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
            ExceptionType = exception.GetType().Name
        };

        // Log based on exception type with structured data
        switch (exception)
        {
            case NotFoundException:
                _logger.LogInformation(exception,
                    "Resource not found. CorrelationId: {CorrelationId}, Path: {RequestPath}, Method: {Method}",
                    correlationId, context.Request.Path.Value, context.Request.Method);
                break;

            case ValidationException:
                _logger.LogWarning(exception,
                    "Validation failed. CorrelationId: {CorrelationId}, Path: {RequestPath}, Method: {Method}",
                    correlationId, context.Request.Path.Value, context.Request.Method);
                break;

            case UnauthorizedException:
                _logger.LogWarning(exception,
                    "Unauthorized access attempt. CorrelationId: {CorrelationId}, Path: {RequestPath}, Method: {Method}, IP: {RemoteIpAddress}",
                    correlationId, context.Request.Path.Value, context.Request.Method,
                    context.Connection.RemoteIpAddress?.ToString());
                break;

            case CrispException:
                _logger.LogError(exception,
                    "Domain exception occurred. CorrelationId: {CorrelationId}, Path: {RequestPath}, Method: {Method}",
                    correlationId, context.Request.Path.Value, context.Request.Method);
                break;

            case OperationCanceledException:
                _logger.LogInformation(exception,
                    "Request was cancelled. CorrelationId: {CorrelationId}, Path: {RequestPath}, Method: {Method}",
                    correlationId, context.Request.Path.Value, context.Request.Method);
                break;

            case TimeoutException:
                _logger.LogError(exception,
                    "Request timed out. CorrelationId: {CorrelationId}, Path: {RequestPath}, Method: {Method}",
                    correlationId, context.Request.Path.Value, context.Request.Method);
                break;

            default:
                _logger.LogError(exception,
                    "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {RequestPath}, Method: {Method}, Type: {ExceptionType}",
                    correlationId, context.Request.Path.Value, context.Request.Method, exception.GetType().Name);
                break;
        }
    }
}
