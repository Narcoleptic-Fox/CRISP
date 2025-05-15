using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace CRISP
{
    /// <summary>
    /// Lightweight middleware that provides consistent error handling and status code mapping
    /// for CRISP endpoints. This complements the standard response types already provided by services.
    /// </summary>
    public class ResponseHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseHandlingMiddleware"/> class.
        /// </summary>
        public ResponseHandlingMiddleware(RequestDelegate next, ILogger<ResponseHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the request and provides global error handling.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Let the request execute through the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception caught by CRISP middleware");

                // Reset response to ensure we can still write to it
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                // Create a CRISP-compliant error response
                Response response = Response.Failure("An unexpected error occurred. Please try again later.");

                // In development, include the exception details
                IWebHostEnvironment? env = context.RequestServices.GetService<IWebHostEnvironment>();
                if (env?.IsDevelopment() == true)
                {
                    response = Response.Failure("An unexpected error occurred.", [ex.Message, ex.StackTrace ?? string.Empty]);
                }

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

    /// <summary>
    /// Extension methods for the ResponseHandlingMiddleware.
    /// </summary>
    public static class ResponseHandlingMiddlewareExtensions
    {
        /// <summary>
        /// Adds the CRISP response handling middleware to the application pipeline.
        /// </summary>
        public static IApplicationBuilder UseCrispErrorHandling(this IApplicationBuilder builder) => builder.UseMiddleware<ResponseHandlingMiddleware>();
    }
}
