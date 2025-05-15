using CRISP.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CRISP
{
    /// <summary>
    /// Interface for endpoints that process commands with no return value.
    /// Provides a standard pattern for handling commands through HTTP endpoints.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle.</typeparam>
    public interface ICommandEndpoint<TCommand> : IEndpoint
        where TCommand : Command
    {
        /// <summary>
        /// Handles the command request, validates it, and processes it through the appropriate service.
        /// </summary>
        /// <param name="request">The command request from the request body.</param>
        /// <param name="service">The service responsible for processing the command.</param>
        /// <param name="validators">Collection of validators for the command.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response.</returns>
        static abstract Task<Response> Handle([FromBody] TCommand request, [FromServices] ICommandService<TCommand> service, [FromServices] IEnumerable<IValidator<TCommand>> validators);
    }

    /// <summary>
    /// Interface for endpoints that process commands and return a result.
    /// Provides a standard pattern for handling commands that produce results through HTTP endpoints.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to handle.</typeparam>
    /// <typeparam name="TResult">The type of result returned by processing the command.</typeparam>
    public interface ICommandEndpoint<TCommand, TResult> : IEndpoint
        where TCommand : Command<TResult>
    {
        /// <summary>
        /// Handles the command request, validates it, and processes it through the appropriate service.
        /// </summary>
        /// <param name="request">The command request from the request body.</param>
        /// <param name="service">The service responsible for processing the command.</param>
        /// <param name="validators">Collection of validators for the command.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response with the result.</returns>
        static abstract Task<Response<TResult>> Handle([FromBody] TCommand request, [FromServices] ICommandService<TCommand, TResult> service, [FromServices] IEnumerable<IValidator<TCommand>> validators);
    }

    /// <summary>
    /// Interface for endpoints that handle entity creation commands.
    /// Specializes the command endpoint for creation operations that return an entity identifier.
    /// </summary>
    /// <typeparam name="TCommand">The type of create command.</typeparam>
    /// <typeparam name="TId">The type of identifier for the created entity.</typeparam>
    public interface ICreateEndpoint<TCommand, TId> : ICommandEndpoint<TCommand, TId>
        where TCommand : CreateCommand<TId>
        where TId : IComparable<TId>
    {
    }

    /// <summary>
    /// Interface for endpoints that handle entity update commands.
    /// Provides a standard pattern for modification operations through HTTP endpoints.
    /// </summary>
    /// <typeparam name="TModify">The type of update command.</typeparam>
    /// <typeparam name="TId">The type of identifier for the entity being modified.</typeparam>
    public interface IModifyEndpoint<TModify, TId> : ICommandEndpoint<TModify>
        where TModify : UpdateCommand<TId>
        where TId : IComparable<TId>
    {
    }

    /// <summary>
    /// Interface for endpoints that handle entity updates by ID.
    /// Supports operations where the ID is provided as part of the route rather than the request body.
    /// </summary>
    /// <typeparam name="TModify">The type of update command.</typeparam>
    /// <typeparam name="TId">The type of identifier for the entity being modified.</typeparam>
    public interface IByIdModifyEndpoint<TModify, TId> : IEndpoint
        where TModify : UpdateCommand<TId>
        where TId : IComparable<TId>
    {
        /// <summary>
        /// Handles the update request for an entity identified by the provided ID.
        /// </summary>
        /// <param name="id">The identifier of the entity to update, extracted from the route.</param>
        /// <param name="service">The service responsible for processing the update command.</param>
        /// <param name="validators">Collection of validators for the update command.</param>
        /// <returns>A task that represents the asynchronous operation, containing the response.</returns>
        static abstract Task<Response> Handle([FromRoute] TId id, [FromServices] ICommandService<TModify> service, [FromServices] IEnumerable<IValidator<TModify>> validators);
    }
}