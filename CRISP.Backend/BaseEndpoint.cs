using CRISP.Validation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CRISP
{
    /// <summary>
    /// Base class that provides common functionality for all endpoint implementations.
    /// Contains shared utility methods and configuration for CRISP endpoints.
    /// </summary>
    /// <remarks>
    /// This class encapsulates cross-cutting concerns that apply to all endpoints,
    /// promoting code reuse and consistency in endpoint implementations.
    /// </remarks>
    public class BaseEndpoint
    {
        /// <summary>
        /// JSON serializer settings used for serializing/deserializing data in endpoints.
        /// Configured to handle circular references by ignoring them.
        /// </summary>
        protected static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        /// <summary>
        /// Validates a request using all registered validators for that request type.
        /// </summary>
        /// <typeparam name="TRequest">The type of request to validate.</typeparam>
        /// <param name="request">The request instance to validate.</param>
        /// <param name="validators">Collection of validators that can validate the request.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> containing the combined results of all validators.
        /// If any validator fails, the result will not be valid.
        /// </returns>
        /// <remarks>
        /// This method executes all validators in parallel for optimal performance and
        /// combines their results into a single validation result.
        /// </remarks>
        protected static async Task<ValidationResult> ValidateRequest<TRequest>(TRequest request, IEnumerable<IValidator<TRequest>> validators)
           where TRequest : IRequest
        {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            IEnumerable<Task<ValidationResult>> validationTasks = validators.Select(async v => v.Validate(request));
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            ValidationResult[] validationResults = await Task.WhenAll(validationTasks);

            return ValidationResult.Combine(validationResults);
        }
    }

    /// <summary>
    /// Base implementation for command endpoints that handle commands with no return value.
    /// Provides standardized validation and command handling.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle.</typeparam>
    /// <remarks>
    /// This class implements the standard pattern for processing commands:
    /// 1. Validate the command using all registered validators
    /// 2. If validation fails, return a bad request response with validation errors
    /// 3. If validation succeeds, process the command through the service
    /// </remarks>
    public abstract class CommandEndpointBase<TCommand> : BaseEndpoint
        where TCommand : Command
    {
        /// <summary>
        /// Handles the command request through validation and processing.
        /// </summary>
        /// <param name="request">The command request from the request body.</param>
        /// <param name="service">The service responsible for processing the command.</param>
        /// <param name="validators">Collection of validators for the command.</param>
        /// <returns>A response indicating success or failure of the command.</returns>
        /// <remarks>
        /// This implementation follows security best practices by:
        /// - Validating input before processing
        /// - Returning standardized error responses
        /// - Delegating command processing to a service layer
        /// </remarks>
        public static async Task<Response> Handle([FromBody] TCommand request, [FromServices] ICommandService<TCommand> service, [FromServices] IEnumerable<IValidator<TCommand>> validators)
        {
            // Validate the command
            ValidationResult validationResult = await ValidateRequest(request, validators);
            if (!validationResult.IsValid)
                return Response.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            // Process the command
            return await service.Send(request);
        }
    }

    /// <summary>
    /// Base implementation for command endpoints that handle commands and return results.
    /// Provides standardized validation and command handling with typed results.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle.</typeparam>
    /// <typeparam name="TResult">The type of result returned by processing the command.</typeparam>
    /// <remarks>
    /// This class implements the standard pattern for processing commands with results:
    /// 1. Validate the command using all registered validators
    /// 2. If validation fails, return a bad request response with validation errors
    /// 3. If validation succeeds, process the command through the service and return the result
    /// </remarks>
    public abstract class CommandEndpointBase<TCommand, TResult> : BaseEndpoint
        where TCommand : Command<TResult>
    {
        /// <summary>
        /// Handles the command request through validation and processing, returning a result.
        /// </summary>
        /// <param name="request">The command request from the request body.</param>
        /// <param name="service">The service responsible for processing the command.</param>
        /// <param name="validators">Collection of validators for the command.</param>
        /// <returns>A response containing the result of processing the command, or validation errors.</returns>
        /// <remarks>
        /// This implementation follows security best practices by:
        /// - Validating input before processing
        /// - Returning standardized error responses
        /// - Delegating command processing to a service layer
        /// - Providing strongly typed results
        /// </remarks>
        public static async Task<Response<TResult>> Handle([FromBody] TCommand request, [FromServices] ICommandService<TCommand, TResult> service, [FromServices] IEnumerable<IValidator<TCommand>> validators)
        {
            // Validate the command
            ValidationResult validationResult = await ValidateRequest(request, validators);
            if (!validationResult.IsValid)
                return Response<TResult>.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            // Process the command
            return await service.Send(request);
        }
    }

    /// <summary>
    /// Base implementation for query endpoints that handle queries and return results.
    /// Provides standardized validation and comquerymand handling with typed results.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to handle.</typeparam>
    /// <typeparam name="TResult">The type of result returned by processing the query.</typeparam>
    /// <remarks>
    /// This class implements the standard pattern for processing queries with results:
    /// 1. Validate the query using all registered validators
    /// 2. If validation fails, return a bad request response with validation errors
    /// 3. If validation succeeds, process the query through the service and return the result
    /// </remarks>
    public abstract class QueryEndpointBase<TQuery, TResult> : BaseEndpoint
        where TQuery : Query<TResult>
    {
        /// <summary>
        /// Handles the query request through validation and processing, returning a result.
        /// </summary>
        /// <param name="request">The query request from the request body.</param>
        /// <param name="service">The service responsible for processing the query.</param>
        /// <param name="validators">Collection of validators for the query.</param>
        /// <returns>A response containing the result of processing the query, or validation errors.</returns>
        /// <remarks>
        /// This implementation follows security best practices by:
        /// - Validating input before processing
        /// - Returning standardized error responses
        /// - Delegating query processing to a service layer
        /// - Providing strongly typed results
        /// </remarks>
        public static async Task<Response<TResult>> Handle([FromQuery] TQuery request, [FromServices] IQueryService<TQuery, TResult> service, [FromServices] IEnumerable<IValidator<TQuery>> validators)
        {
            // Validate the query
            ValidationResult validationResult = await ValidateRequest(request, validators);
            if (!validationResult.IsValid)
                return Response<TResult>.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            // Process the query
            return await service.Send(request);
        }
    }
}
