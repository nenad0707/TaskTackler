using Microsoft.JSInterop;
using System.Net;
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
        return $"Todos?pageNumber={pageNumber}&pageSize={pageSize}";
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
            var latestLoad = await GetTodosAsync(1, 5);
            var totalPages = latestLoad.TotalPages;
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

            var totalPages = await GetTotalPagesAsync();
            var lastPageUriKey = GenerateUriKey(totalPages, 5);
            var lastPageData = await _cacheManager.GetItemAsync($"data-{lastPageUriKey}");

            if (!string.IsNullOrEmpty(lastPageData))
            {
                var lastPageApiResponse = JsonSerializer.Deserialize<ApiResponse>(lastPageData);
                if (lastPageApiResponse != null && !lastPageApiResponse.Todos.Any())
                {
                    await _cacheManager.RemoveItemAsync($"data-{lastPageUriKey}");
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"etag-{lastPageUriKey}");
                }
            }
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkTodoAsCompletedAsync(TodoModel todo)
    {
        var response = await _httpClient.PutAsJsonAsync<TodoModel>($"Todos/{todo.Id}/Complete", todo);
        if (response.IsSuccessStatusCode)
        {
            await ClearCacheForAffectedPages(todo.Id);
            var pageNumber = GetPageNumberForTodoId(todo.Id);
            await GetTodosAsync(pageNumber, 5);
        }
        return response.IsSuccessStatusCode;
    }

    private async Task ClearCacheForAffectedPages(int todoId)
    {
        const int maxPages = 100;
        const int pageSize = 5;

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

                    if (apiResponse.Todos.Count == 1)
                    {
                        await _cacheManager.RemoveItemAsync($"data-{uriKey}");
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"etag-{uriKey}");
                    }
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
        int pageNumber = (todoId / pageSize) + 1;
        return pageNumber;
    }

    public async Task<int> GetTotalPagesAsync()
    {
        var response = await _httpClient.GetAsync("Todos?pageNumber=1&pageSize=1"); 

        if (response.IsSuccessStatusCode)
        {
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            return apiResponse!.TotalPages;
        }

        return 0;
    }
}
