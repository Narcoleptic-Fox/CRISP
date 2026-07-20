namespace CRISP.Core.Common
{
    /// <summary>
    /// Defines a marker interface for commands that do not return a response.
    /// </summary>
    /// <remarks>
    /// Commands represent write operations that modify system state.
    /// They follow the CQRS (Command Query Responsibility Segregation) pattern,
    /// separating write operations from read operations.
    /// This interface is used for commands that perform actions without returning data.
    /// </remarks>
    public interface ICommand;

    /// <summary>
    /// Defines a contract for services that execute commands without returning a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to execute. Must implement <see cref="ICommand"/>.</typeparam>
    /// <remarks>
    /// Command services implement the command handling logic in the CQRS pattern.
    /// They are responsible for processing commands and performing the necessary
    /// state changes or side effects in the system.
    /// </remarks>
    public interface ICommandService<TCommand>
        where TCommand : ICommand
    {
        /// <summary>
        /// Executes the specified command without expecting a response.
        /// </summary>
        /// <param name="command">The command instance containing the parameters for the operation.</param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the task to complete.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask"/> that represents the asynchronous command execution.
        /// The task completes when the command has been successfully processed.
        /// </returns>
        /// <remarks>
        /// This method should be implemented to handle the specific command logic and perform
        /// any necessary state changes. The operation may have side effects and modify system state.
        /// </remarks>
        ValueTask Send(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines a marker interface for commands that return a response of the specified type.
    /// </summary>
    /// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
    /// <remarks>
    /// This interface is used for commands that perform actions and return data,
    /// such as creation commands that return the ID of the created entity.
    /// </remarks>
    public interface ICommand<out TResponse>;

    /// <summary>
    /// Defines a contract for services that execute commands and return a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to execute. Must implement <see cref="ICommand{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
    /// <remarks>
    /// Command services implement the command handling logic in the CQRS pattern for commands
    /// that need to return data after processing, such as creation operations.
    /// </remarks>
    public interface ICommandService<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        /// <summary>
        /// Executes the specified command and returns a response.
        /// </summary>
        /// <param name="command">The command instance containing the parameters for the operation.</param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the task to complete.
        /// The default value is <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTask{TResponse}"/> that represents the asynchronous command execution.
        /// The task result contains the response data from the command processing.
        /// </returns>
        /// <remarks>
        /// This method should be implemented to handle the specific command logic, perform
        /// any necessary state changes, and return the appropriate response data.
        /// </remarks>
        ValueTask<TResponse> Send(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Represents a base command for creating new entities.
    /// </summary>
    /// <remarks>
    /// Create commands are used to add new entities to the system and return the ID
    /// of the newly created entity. This abstract record provides a foundation
    /// for implementing specific creation commands.
    /// </remarks>
    public abstract record CreateCommand : ICommand<Guid>;

    /// <summary>
    /// Defines a contract for services that handle entity creation commands.
    /// </summary>
    /// <typeparam name="TCreate">The type of create command. Must inherit from <see cref="CreateCommand"/>.</typeparam>
    /// <remarks>
    /// Create services implement the logic for creating new entities and returning
    /// their unique identifiers. They extend the command service pattern specifically
    /// for creation operations.
    /// </remarks>
    public interface ICreateService<TCreate> : ICommandService<TCreate, Guid>
        where TCreate : CreateCommand
    {
    }

    /// <summary>
    /// Represents a base command for modifying existing entities.
    /// </summary>
    /// <remarks>
    /// Modify commands are used to update existing entities in the system.
    /// This abstract record provides a foundation for implementing specific
    /// modification commands with entity identification.
    /// </remarks>
    [ExportTsInterface]
    public abstract record ModifyCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyCommand"/> record.
        /// </summary>
        public ModifyCommand() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyCommand"/> record for the specified entity.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to modify.</param>
        public ModifyCommand(Guid id) => Id = id;

        /// <summary>
        /// Gets or sets the unique identifier of the entity to modify.
        /// </summary>
        /// <value>
        /// A <see cref="Guid"/> representing the unique identifier of the entity to be modified.
        /// </value>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Defines a contract for services that modify existing entities.
    /// </summary>
    /// <typeparam name="TModify">The type of modify command. Must inherit from <see cref="ModifyCommand"/>.</typeparam>
    /// <remarks>
    /// Modify services implement the logic for modifying existing entities.
    /// They extend the command service pattern specifically for modification operations.
    /// </remarks>
    public interface IModifyService<TModify> : ICommandService<TModify>
        where TModify : ModifyCommand
    {
    }

    /// <summary>
    /// Represents a command for permanently deleting an entity from the system.
    /// </summary>
    /// <param name="Id">The unique identifier of the entity to delete.</param>
    /// <remarks>
    /// Delete commands represent a remove operation. Individual handlers determine whether
    /// that removal is permanent or translated into a domain-specific archival action.
    /// </remarks>
    public sealed record DeleteCommand(Guid Id) : ModifyCommand(Id);

    /// <summary>
    /// Represents a command for archiving an entity with a business reason.
    /// </summary>
    /// <param name="Id">The unique identifier of the entity to archive.</param>
    /// <param name="ArchiveReason">The reason the entity is being archived.</param>
    public sealed record ArchiveCommand(Guid Id, string ArchiveReason) : ModifyCommand(Id);

    /// <summary>
    /// Validates archive commands before an entity is marked as archived.
    /// </summary>
    public sealed class ArchiveCommandValidator : AbstractValidator<ArchiveCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveCommandValidator"/> class.
        /// </summary>
        public ArchiveCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Entity ID is required.");
            RuleFor(x => x.ArchiveReason)
                .NotEmpty().WithMessage("Archive reason is required.")
                .MaximumLength(500).WithMessage("Archive reason must not exceed 500 characters.");
        }
    }
}
