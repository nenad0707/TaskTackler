using TaskTackler.Models;

namespace TaskTackler.Services;

public interface ITaskService
{

    Task<List<TodoModel>> GetTodosAsync();
    Task<bool> AddTodoAsync(TodoModel todo);
    Task<bool> UpdateTodoAsync(TodoModel todo);
    Task<bool> DeleteTodoAsync(TodoModel todo);
    Task<bool> MarkTodoAsCompletedAsync(TodoModel todo);
}
