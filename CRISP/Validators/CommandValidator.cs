using CRISP.Requests;
using FluentValidation;

namespace CRISP.Validators
{
    /// <summary>
    /// Base validator for command operations that don't return a response.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to validate.</typeparam>
    /// <remarks>
    /// Provides a foundation for implementing validation rules for void commands.
    /// </remarks>
    internal abstract class CommandValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : Command
    {
    }

    /// <summary>
    /// Base validator for command operations that return a specific response type.
    /// </summary>
    /// <typeparam name="TCommand">The type of command to validate.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the command.</typeparam>
    /// <remarks>
    /// Provides a foundation for implementing validation rules for commands that return data.
    /// </remarks>
    internal abstract class CommandValidator<TCommand, TResponse> : AbstractValidator<TCommand>
        where TCommand : Command<TResponse>
    {
    }

    /// <summary>
    /// Validator for commands that create new entities and return their identifiers.
    /// </summary>
    /// <typeparam name="TCommand">The type of create command to validate.</typeparam>
    /// <typeparam name="TId">The type of identifier returned after creation.</typeparam>
    /// <remarks>
    /// Use this validator for implementing validation rules specific to entity creation operations.
    /// </remarks>
    internal abstract class CreateCommandValidator<TCommand, TId> : CommandValidator<TCommand, TId>
        where TCommand : CreateCommand<TId>
        where TId : IEqualityComparer<TId>
    {
    }

    /// <summary>
    /// Validator for commands that modify existing entities.
    /// </summary>
    /// <typeparam name="TCommand">The type of modify command to validate.</typeparam>
    /// <typeparam name="TId">The type of identifier of the entity to modify.</typeparam>
    /// <remarks>
    /// Includes built-in validation to ensure the entity identifier is provided.
    /// </remarks>
    internal abstract class ModifyCommandValidator<TCommand, TId> : CommandValidator<TCommand>
        where TCommand : ModifyCommand<TId>
        where TId : IEqualityComparer<TId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModifyCommandValidator{TCommand, TId}"/> class
        /// with validation for the entity identifier.
        /// </summary>
        public ModifyCommandValidator() => RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
    }

    /// <summary>
    /// Validator for commands that delete entities.
    /// </summary>
    /// <typeparam name="TCommand">The type of delete command to validate.</typeparam>
    /// <typeparam name="TId">The type of identifier of the entity to delete.</typeparam>
    /// <remarks>
    /// Includes built-in validation to ensure the entity identifier is provided.
    /// </remarks>
    internal abstract class DeleteCommandValidator<TCommand, TId> : CommandValidator<TCommand>
        where TCommand : DeleteCommand<TId>
        where TId : IEqualityComparer<TId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteCommandValidator{TCommand, TId}"/> class
        /// with validation for the entity identifier.
        /// </summary>
        public DeleteCommandValidator() => RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
    }

    /// <summary>
    /// Validator for commands that archive entities (soft-delete).
    /// </summary>
    /// <typeparam name="TCommand">The type of archive command to validate.</typeparam>
    /// <typeparam name="TId">The type of identifier of the entity to archive.</typeparam>
    /// <remarks>
    /// Includes built-in validation to ensure the entity identifier is provided.
    /// </remarks>
    internal abstract class ArchiveCommandValidator<TCommand, TId> : CommandValidator<TCommand>
        where TCommand : ArchiveCommand<TId>
        where TId : IEqualityComparer<TId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveCommandValidator{TCommand, TId}"/> class
        /// with validation for the entity identifier.
        /// </summary>
        public ArchiveCommandValidator() => RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
    }
}
