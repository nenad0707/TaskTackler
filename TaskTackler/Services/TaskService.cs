using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TaskTackler.Models;

namespace TaskTackler.Services;

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;
    private readonly CacheManager _cacheManager;

    public TaskService(IHttpClientFactory httpClientFactory, CacheManager cacheManager)
    {
        _httpClient = httpClientFactory.CreateClient("api");
        _cacheManager = cacheManager;
    }

    public async Task<PaginatedResponse<List<TodoModel>>> GetTodosAsync(int pageNumber, int pageSize)
    {
        var uriKey = $"todos?pageNumber={pageNumber}&pageSize={pageSize}";
        var response = await _httpClient.GetAsync(uriKey);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            await _cacheManager.SetItemAsync($"data-{uriKey}", JsonSerializer.Serialize(apiResponse));
            return new PaginatedResponse<List<TodoModel>>
            {
                Data = apiResponse?.Todos,
                TotalPages = apiResponse!.TotalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            };
        }
        else if (response.StatusCode == HttpStatusCode.NotModified)
        {
            var cachedData = await _cacheManager.GetItemAsync($"data-{uriKey}");
            if (!string.IsNullOrEmpty(cachedData))
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(cachedData);
                return new PaginatedResponse<List<TodoModel>>
                {
                    Data = apiResponse?.Todos,
                    TotalPages = apiResponse!.TotalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        return new PaginatedResponse<List<TodoModel>>
        {
            Data = new List<TodoModel>(), // Prazna lista jer nema podataka
            TotalPages = 0, // Nema stranica jer nema podataka
            CurrentPage = pageNumber, // Vraćamo traženi broj stranice
            PageSize = pageSize // Vraćamo traženu veličinu stranice
        };
    }

    public async Task<bool> AddTodoAsync(TodoModel todo)
    {
        var response = await _httpClient.PostAsJsonAsync("Todos", todo.Task);
        if (response.IsSuccessStatusCode)
        {
            await ClearCacheForLastPage();
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateTodoAsync(TodoModel todo)
    {
        var response = await _httpClient.PutAsJsonAsync($"Todos/{todo.Id}", todo.Task);
        if (response.IsSuccessStatusCode)
        {
            await ClearCacheForAffectedPages(todo.Id);
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTodoAsync(TodoModel todo)
    {
        var response = await _httpClient.DeleteAsync($"Todos/{todo.Id}");
        if (response.IsSuccessStatusCode)
        {
            await ClearCacheForAffectedPages(todo.Id);
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkTodoAsCompletedAsync(TodoModel todo)
    {
        var response = await _httpClient.PutAsJsonAsync<TodoModel>($"Todos/{todo.Id}/Complete", todo);
        if (response.IsSuccessStatusCode)
        {
            await ClearCacheForAffectedPages(todo.Id);
        }
        return response.IsSuccessStatusCode;
    }

    private async Task ClearCacheForAffectedPages(int todoId)
    {
        const int maxPages = 100; // Maksimalan broj stranica (prilagodite prema potrebi)
        const int pageSize = 5; // Veličina stranice (prilagodite prema potrebi)

        for (int pageNumber = 1; pageNumber <= maxPages; pageNumber++)
        {
            var uriKey = $"todos?pageNumber={pageNumber}&pageSize={pageSize}";
            var cachedData = await _cacheManager.GetItemAsync($"data-{uriKey}");

            if (!string.IsNullOrEmpty(cachedData))
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(cachedData);
                if (apiResponse != null && apiResponse.Todos.Any(todo => todo.Id == todoId))
                {
                    await _cacheManager.ClearSpecificItemsAsync(uriKey);
                }
            }
        }
    }

    private async Task ClearCacheForLastPage()
    {
        const int maxPages = 100;
        const int pageSize = 5;

        for (int pageNumber = maxPages; pageNumber >= 1; pageNumber--)
        {
            var uriKey = $"todos?pageNumber={pageNumber}&pageSize={pageSize}";
            var cachedData = await _cacheManager.GetItemAsync($"data-{uriKey}");

            if (!string.IsNullOrEmpty(cachedData))
            {
                await _cacheManager.ClearSpecificItemsAsync(uriKey);
                break;
            }
        }
    }
}

