using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskTackler.Models;
using System.Threading.Tasks;
using TaskTackler.Cache;
using TaskTackler.Helpers;
using System.Net.Http.Headers;

namespace TaskTackler.Services
{
    public class TaskService : ITaskService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheInvalidationService _cacheInvalidationService;
        private readonly CacheManager _cacheManager;

        public TaskService(IHttpClientFactory httpClientFactory, ICacheInvalidationService cacheInvalidationService, CacheManager cacheManager)
        {
            _httpClient = httpClientFactory.CreateClient("api");
            _cacheInvalidationService = cacheInvalidationService;
            _cacheManager = cacheManager;
        }

        public static string GenerateUriKey(int pageNumber, int pageSize)
        {
            return $"Todos?pageNumber={pageNumber}&pageSize={pageSize}";
        }

        public async Task<PaginatedResponse<List<TodoModel>>> GetTodosAsync(int pageNumber, int pageSize)
        {
            var uriKey = UrlKeyGenerator.GenerateUriKey(pageNumber, pageSize);
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
                Data = [],
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
                await _cacheInvalidationService.ClearCacheForLastPage();
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTodoAsync(TodoModel todo)
        {
            var response = await _httpClient.PutAsJsonAsync($"Todos/{todo.Id}", todo.Task);
            if (response.IsSuccessStatusCode)
            {
                await _cacheInvalidationService.ClearCacheForAffectedPages(todo.Id);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTodoAsync(TodoModel todo)
        {
            var response = await _httpClient.DeleteAsync($"Todos/{todo.Id}");
            if (response.IsSuccessStatusCode)
            {
                await _cacheInvalidationService.ClearCacheForAffectedPages(todo.Id);
                await _cacheInvalidationService.ClearCacheForLastPage();

                //// Optionally: Fetch and update the totalPages
                //var totalPages = await GetTotalPagesAsync();
                //if (totalPages < currentPage)
                //{
                //    currentPage = totalPages;
                //}
            }
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> MarkTodoAsCompletedAsync(TodoModel todo)
        {
            var response = await _httpClient.PutAsJsonAsync<TodoModel>($"Todos/{todo.Id}/Complete", todo);
            if (response.IsSuccessStatusCode)
            {
                await _cacheInvalidationService.ClearCacheForAffectedPages(todo.Id);
            }
            return response.IsSuccessStatusCode;
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
}
