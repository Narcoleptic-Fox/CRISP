using TodoApi.Features.Todo.Models;

namespace TodoApi.Features.Todo;

public interface ITodoRepository
{
    Task<TodoEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TodoEntity>> GetAllAsync();
    Task<TodoEntity> CreateAsync(TodoEntity todo);
    Task<TodoEntity?> UpdateAsync(int id, TodoEntity todo);
    Task<bool> DeleteAsync(int id);
}