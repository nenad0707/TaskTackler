﻿using Microsoft.JSInterop;

namespace TaskTackler.Cache;

public class CacheManager
{
    private readonly IJSRuntime _jsRuntime;

    public CacheManager(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task<string> GetItemAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }

    public async Task ClearSpecificItemsAsync(string uriKey)
    {
        await Task.WhenAll(
            RemoveItemAsync($"etag-{uriKey}"),
            RemoveItemAsync($"data-{uriKey}")
        );
    }
}
