using Microsoft.JSInterop;

namespace TaskTackler.Services;

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
        await RemoveItemAsync($"etag-{uriKey}");
        await RemoveItemAsync($"data-{uriKey}");
    }

    public async Task<List<string>> GetAllKeysAsync()
    {
        var keys = await _jsRuntime.InvokeAsync<string[]>("Object.keys", new { });
        return keys.ToList();
    }
}

