using Crisp.Commands;

namespace TodoApi.Features.Todo.Commands;

public record DeleteTodoCommand(int Id) : ICommand<bool>;

public class DeleteTodo : ICommandHandler<DeleteTodoCommand, bool>
{
    private readonly ITodoRepository _repository;

    public DeleteTodo(ITodoRepository repository) => _repository = repository;

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken) => await _repository.DeleteAsync(request.Id);
}