using System.Collections.Generic;
using System.Threading.Tasks;
using TaskTackler.Models;

namespace TaskTackler.Services
{
    public interface ITaskService
    {
        Task<PaginatedResponse<List<TodoModel>>> GetTodosAsync(int pageNumber, int pageSize);
        Task<bool> AddTodoAsync(TodoModel todo);
        Task<bool> UpdateTodoAsync(TodoModel todo);
        Task<(bool IsSuccess, int UpdatedPage)> DeleteTodoAsync(TodoModel todo, int currentPage);
        Task<bool> MarkTodoAsCompletedAsync(TodoModel todo);
        Task<int> GetTotalPagesAsync();
    }
}
