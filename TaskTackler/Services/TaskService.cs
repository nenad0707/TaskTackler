using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskTackler.Models;

namespace TaskTackler.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;

    public TaskService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("api");
    }

    public async Task<PaginatedResponse<List<TodoModel>>> GetTodosAsync(int pageNumber, int pageSize)
    {
        var response = await _httpClient.GetAsync($"todos?pageNumber={pageNumber}&pageSize={pageSize}");
        if (response.IsSuccessStatusCode)
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return new PaginatedResponse<List<TodoModel>>
            {
                Data = apiResponse?.Todos,
                TotalPages = apiResponse!.TotalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }
        return new PaginatedResponse<List<TodoModel>>
        {
            Data = new List<TodoModel>(),
            TotalPages = 0,
            CurrentPage = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> AddTodoAsync(TodoModel todo)
    {
        var response = await _httpClient.PostAsJsonAsync("Todos", todo.Task);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateTodoAsync(TodoModel todo)
    {
        var response = await _httpClient.PutAsJsonAsync($"Todos/{todo.Id}", todo.Task);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTodoAsync(TodoModel todo)
    {
        var response = await _httpClient.DeleteAsync($"Todos/{todo.Id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkTodoAsCompletedAsync(TodoModel todo)
    {
        var response = await _httpClient.PutAsJsonAsync<TodoModel>($"Todos/{todo.Id}/Complete", todo);
        return response.IsSuccessStatusCode;
    }
}
