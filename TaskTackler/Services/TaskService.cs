using Microsoft.JSInterop;
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
    private readonly IJSRuntime _jsRuntime;

    public TaskService(IHttpClientFactory httpClientFactory, CacheManager cacheManager, IJSRuntime jsRuntime)
    {
        _httpClient = httpClientFactory.CreateClient("api");
        _cacheManager = cacheManager;
        _jsRuntime = jsRuntime;
    }

    public static string GenerateUriKey(int pageNumber, int pageSize)
    {
        return $"todos?pageNumber={pageNumber}&pageSize={pageSize}";
    }

    public async Task<PaginatedResponse<List<TodoModel>>> GetTodosAsync(int pageNumber, int pageSize)
    {
        var uriKey = GenerateUriKey(pageNumber, pageSize);
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
            Data = new List<TodoModel>(),
            TotalPages = 0,
            CurrentPage = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<bool> AddTodoAsync(TodoModel todo)
    {
        var response = await _httpClient.PostAsJsonAsync("Todos", todo.Task);
        if (response.IsSuccessStatusCode)
        {
            await ClearCacheForLastPage();
            // Get total pages after adding the new task
            var latestLoad = await GetTodosAsync(1, 5);
            var totalPages = latestLoad.TotalPages;
            // Cache the new last page
            await GetTodosAsync(totalPages, 5);
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateTodoAsync(TodoModel todo)
    {
        var response = await _httpClient.PutAsJsonAsync($"Todos/{todo.Id}", todo.Task);
        if (response.IsSuccessStatusCode)
        {
            await ClearCacheForAffectedPages(todo.Id);
            // Refresh the current page after updating
            var pageNumber = GetPageNumberForTodoId(todo.Id);
            await GetTodosAsync(pageNumber, 5);
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
            // Refresh the current page after marking as completed
            var pageNumber = GetPageNumberForTodoId(todo.Id);
            await GetTodosAsync(pageNumber, 5);
        }
        return response.IsSuccessStatusCode;
    }

    private async Task ClearCacheForAffectedPages(int todoId)
    {
        const int maxPages = 100; // Maksimalan broj stranica (prilagodite prema potrebi)
        const int pageSize = 5; // Veličina stranice (prilagodite prema potrebi)

        for (int pageNumber = 1; pageNumber <= maxPages; pageNumber++)
        {
            var uriKey = GenerateUriKey(pageNumber, pageSize);
            var cachedData = await _cacheManager.GetItemAsync($"data-{uriKey}");

            if (!string.IsNullOrEmpty(cachedData))
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(cachedData);
                if (apiResponse != null && apiResponse.Todos.Any(todo => todo.Id == todoId))
                {
                    await _cacheManager.RemoveItemAsync($"data-{uriKey}");
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"etag-{uriKey}");
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
            var uriKey = GenerateUriKey(pageNumber, pageSize);
            var cachedData = await _cacheManager.GetItemAsync($"data-{uriKey}");

            if (!string.IsNullOrEmpty(cachedData))
            {
                await _cacheManager.RemoveItemAsync($"data-{uriKey}");
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"etag-{uriKey}");
                break;
            }
        }
    }

    private int GetPageNumberForTodoId(int todoId)
    {
        const int pageSize = 5;
        // Pronađite odgovarajući pageNumber za dati todoId (ovo je primer implementacije, može se prilagoditi prema potrebi)
        // Pretpostavljamo da svaki zadatak ima jedinstveni ID i da se zadaci učitavaju po redosledu ID-a
        int pageNumber = (todoId / pageSize) + 1;
        return pageNumber;
    }
}
