using CRISP.Requests;
using FluentValidation;

namespace CRISP.Validators
{
    internal abstract class CommandValidator<TCommand> : AbstractValidator<TCommand>
        where TCommand : Command
    {
    }

    internal abstract class CommandValidator<TCommand, TResponse> : AbstractValidator<TCommand>
        where TCommand : Command<TResponse>
    {
    }

    internal abstract class CreateCommandValidator<TCommand, TId> : CommandValidator<TCommand, TId>
        where TCommand : CreateCommand<TId>
        where TId : IEqualityComparer<TId>
    {
    }

    internal abstract class ModifyCommandValidator<TCommand, TId> : CommandValidator<TCommand>
        where TCommand : ModifyCommand<TId>
        where TId : IEqualityComparer<TId>
    {
        public ModifyCommandValidator() => RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
    }

    internal abstract class DeleteCommandValidator<TCommand, TId> : CommandValidator<TCommand>
        where TCommand : DeleteCommand<TId>
        where TId : IEqualityComparer<TId>
    {
        public DeleteCommandValidator() => RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
    }

    internal abstract class ArchiveCommandValidator<TCommand, TId> : CommandValidator<TCommand>
        where TCommand : ArchiveCommand<TId>
        where TId : IEqualityComparer<TId>
    {
        public ArchiveCommandValidator() => RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
    }
}
