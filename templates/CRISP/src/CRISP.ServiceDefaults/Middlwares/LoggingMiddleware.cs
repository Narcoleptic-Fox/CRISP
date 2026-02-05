using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CRISP.ServiceDefaults.Middlwares
{
    public sealed class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<LoggingMiddleware> logger)
        {
            logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

            await _next(context);

            logger.LogInformation($"Response: {context.Response.StatusCode}");
        }
    }
}