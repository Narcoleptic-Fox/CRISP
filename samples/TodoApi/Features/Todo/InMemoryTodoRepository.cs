using System.Collections.Concurrent;
using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<int, TodoEntity> _todos = new();
    private int _nextId = 1;

    public Task<TodoEntity?> GetByIdAsync(int id)
    {
        _todos.TryGetValue(id, out TodoEntity? todo);
        return Task.FromResult(todo);
    }

    public Task<IEnumerable<TodoEntity>> GetAllAsync() => Task.FromResult(_todos.Values.AsEnumerable());

    public Task<TodoEntity> CreateAsync(TodoEntity todo)
    {
        todo.Id = Interlocked.Increment(ref _nextId);
        todo.CreatedAt = DateTime.UtcNow;
        _todos[todo.Id] = todo;
        return Task.FromResult(todo);
    }

    public Task<TodoEntity?> UpdateAsync(int id, TodoEntity updatedTodo)
    {
        if (!_todos.TryGetValue(id, out TodoEntity? existingTodo))
            return Task.FromResult<TodoEntity?>(null);

        existingTodo.Title = updatedTodo.Title;
        existingTodo.Description = updatedTodo.Description;

        if (updatedTodo.IsCompleted && !existingTodo.IsCompleted)
            existingTodo.CompletedAt = DateTime.UtcNow;
        else if (!updatedTodo.IsCompleted && existingTodo.IsCompleted)
        {
            existingTodo.CompletedAt = null;
        }

        existingTodo.IsCompleted = updatedTodo.IsCompleted;

        return Task.FromResult<TodoEntity?>(existingTodo);
    }

    public Task<bool> DeleteAsync(int id) => Task.FromResult(_todos.TryRemove(id, out _));

    // Security.ITodoRepository implementation
    public Task<List<TodoEntity>> GetTodosByUserAsync(string userId, CancellationToken cancellationToken)
    {
        // For demo purposes, return all todos. In a real app, todos would have a UserId property.
        List<TodoEntity> result = _todos.Values.ToList();
        return Task.FromResult(result);
    }
}