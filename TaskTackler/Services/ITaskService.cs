using TaskTackler.Models;

namespace TaskTackler.Services;

public interface ITaskService
{
    Task<PaginatedResponse<List<TodoModel>>> GetTodosAsync(int pageNumber, int pageSize);
    Task<bool> AddTodoAsync(TodoModel todo);
    Task<bool> UpdateTodoAsync(TodoModel todo);
    Task<bool> DeleteTodoAsync(TodoModel todo);
    Task<bool> MarkTodoAsCompletedAsync(TodoModel todo);
}
