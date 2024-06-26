﻿using System.Text.Json;
using TaskTackler.Helpers;
using TaskTackler.Models;

namespace TaskTackler.Cache
{
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly CacheManager _cacheManager;

        public CacheInvalidationService(CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task ClearCacheForAffectedPages(int todoId)
        {
            const int pageSize = 5;
            const int maxPages = 100;

            for (int pageNumber = 1; pageNumber <= maxPages; pageNumber++)
            {
                var uriKey = UrlKeyGenerator.GenerateUriKey(pageNumber, pageSize);
                var cachedData = await _cacheManager.GetItemAsync($"data-{uriKey}");

                if (!string.IsNullOrEmpty(cachedData))
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(cachedData);
                    if (apiResponse != null && apiResponse.Todos!.Any(todo => todo.Id == todoId))
                    {
                        await _cacheManager.ClearSpecificItemsAsync(uriKey);
                    }
                }
            }
        }

        public async Task ClearCacheForLastPage()
        {
            const int pageSize = 5;
            const int maxPages = 100;

            for (int pageNumber = maxPages; pageNumber >= 1; pageNumber--)
            {
                var uriKey = UrlKeyGenerator.GenerateUriKey(pageNumber, pageSize);
                var cachedData = await _cacheManager.GetItemAsync($"data-{uriKey}");

                if (!string.IsNullOrEmpty(cachedData))
                {
                    await _cacheManager.ClearSpecificItemsAsync(uriKey);
                    break;
                }
            }
        }

        public async Task ClearCacheForAllPages()
        {
            const int pageSize = 5;
            const int maxPages = 100;

            for (int pageNumber = 1; pageNumber <= maxPages; pageNumber++)
            {
                var uriKey = UrlKeyGenerator.GenerateUriKey(pageNumber, pageSize);
                await _cacheManager.ClearSpecificItemsAsync(uriKey);
            }
        }

        public async Task ClearCacheForSpecificPage(int pageNumber)
        {
            const int pageSize = 5;
            var uriKey = UrlKeyGenerator.GenerateUriKey(pageNumber, pageSize);
            await _cacheManager.ClearSpecificItemsAsync(uriKey);
        }
    }
}
