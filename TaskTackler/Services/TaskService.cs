using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskTackler.Models;

namespace TaskTackler.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public TaskService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _authService = authService;
        _httpClient = httpClientFactory.CreateClient("api");
    }

    public async Task<List<TodoModel>> GetTodosAsync()
    {
        var token = await _authService.GetTokenAsync();
        var response = await _httpClient.GetAsync("todos");
        //Console.WriteLine(response.Content);
        if (response.IsSuccessStatusCode)
        {
            return await _httpClient.GetFromJsonAsync<List<TodoModel>>("Todos") ?? new List<TodoModel>();
        }
        return new List<TodoModel>();
    }

    public async Task<bool> AddTodoAsync(TodoModel todo)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _httpClient.PostAsJsonAsync("Todos", todo.Task);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateTodoAsync(TodoModel todo)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _httpClient.PutAsJsonAsync($"Todos/{todo.Id}", todo.Task);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTodoAsync(TodoModel todo)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _httpClient.DeleteAsync($"Todos/{todo.Id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkTodoAsCompletedAsync(TodoModel todo)
    {
        var token = await _authService.GetTokenAsync();
        var response = await _httpClient.PutAsJsonAsync<TodoModel>($"Todos/{todo.Id}/Complete", todo);
        Console.WriteLine(response);
        return response.IsSuccessStatusCode;
    }
}
