using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using TaskTackler.Services;

namespace TaskTackler.Handlers;

public class CashingHandler : DelegatingHandler
{
    private readonly CacheManager _cacheManager;
    private readonly IJSRuntime _jsRuntime;

    public CashingHandler(CacheManager cacheManager, IJSRuntime jsRuntime)
    {
        _cacheManager = cacheManager;
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var queryParams = HttpUtility.ParseQueryString(request.RequestUri!.Query);
        int pageNumber = int.TryParse(queryParams["pageNumber"], out int tempPageNumber) ? tempPageNumber : 1;
        int pageSize = int.TryParse(queryParams["pageSize"], out int tempPageSize) ? tempPageSize : 5;
        string uriKey = TaskService.GenerateUriKey(pageNumber, pageSize);
        Console.WriteLine("[CashingHandler] Sending request.");

        if (request.Method != HttpMethod.Get)
        {
            await ClearETagAndDataKeysAsync(uriKey);
            Console.WriteLine("[CashingHandler] Specific ETags and data cleared.");
        }

        if (request.Method == HttpMethod.Get)
        {
            var etag = await _cacheManager.GetItemAsync($"etag-{uriKey}");
            if (!string.IsNullOrEmpty(etag))
            {
                etag = etag.Replace("W/", "").Trim();
                if (!etag.StartsWith("\""))
                {
                    etag = $"\"{etag}\"";
                }
                Console.WriteLine($"[CashingHandler] Using ETag from localStorage: {etag}");
                request.Headers.IfNoneMatch.Clear();
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag, false));
            }
        }

        var response = await base.SendAsync(request, cancellationToken);
        Console.WriteLine("[CashingHandler] Response received.");

        if (response.StatusCode == HttpStatusCode.OK && request.Method == HttpMethod.Get)
        {
            var content = await response.Content.ReadAsStringAsync();
            await _cacheManager.SetItemAsync($"data-{uriKey}", content);
            Console.WriteLine($"[CashingHandler] Data saved.");

            if (response.Headers.ETag != null)
            {
                var formattedETag = response.Headers.ETag.Tag;
                if (!formattedETag.StartsWith("\""))
                {
                    formattedETag = $"\"{formattedETag}\"";
                }
                await _cacheManager.SetItemAsync($"etag-{uriKey}", formattedETag);
                Console.WriteLine($"[CashingHandler] ETag saved: {formattedETag}");
            }
        }

        if (response.StatusCode == HttpStatusCode.NotModified && request.Method == HttpMethod.Get)
        {
            var cachedData = await _cacheManager.GetItemAsync($"data-{uriKey}");
            if (!string.IsNullOrEmpty(cachedData))
            {
                var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(cachedData, Encoding.UTF8, "application/json")
                };
                return httpResponse;
            }
        }

        return response;
    }

    private async Task ClearETagAndDataKeysAsync(string uriKey)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"etag-{uriKey}");
        await _cacheManager.RemoveItemAsync($"data-{uriKey}");
    }
}
