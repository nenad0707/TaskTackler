using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace TaskTackler.Handlers
{
    public class CashingHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;

        public CashingHandler(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string uriKey = request.RequestUri!.ToString();
            Console.WriteLine("[CashingHandler] Sending request.");

            if (request.Method != HttpMethod.Get)
            {
                var keys = await _jsRuntime.InvokeAsync<string[]>("Object.keys", new { prefix = "localStorage" });
                foreach (var key in keys)
                {
                    if (key.StartsWith("etag-https://localhost:7213/api/todos"))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
                    }
                }
                Console.WriteLine("[CashingHandler] All Todo-related ETags cleared.");
            }

            if (request.Method == HttpMethod.Get)
            {
                var etag = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", $"etag-{uriKey}");
                if (!string.IsNullOrEmpty(etag))
                {
                    // Ensure ETag is formatted correctly by removing any weak ETag prefix and wrapping in quotes
                    etag = etag.Replace("W/", "").Trim();
                    if (!etag.StartsWith("\""))
                    {
                        etag = $"\"{etag}\"";
                    }
                    Console.WriteLine($"[CashingHandler] Using ETag from localStorage: {etag}");
                    request.Headers.IfNoneMatch.Clear();
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag,true));
                }
            }

            var response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine("[CashingHandler] Response received.");

            // Sačuvati novi ETag u localStorage ako je dostupan
            if (response.Headers.ETag != null && request.Method == HttpMethod.Get)
            {
                var formattedETag = response.Headers.ETag.Tag;
                if (!formattedETag.StartsWith("\""))
                {
                    formattedETag = $"\"{formattedETag}\"";
                }
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"etag-{uriKey}", formattedETag);
                Console.WriteLine($"[CashingHandler] ETag saved: {formattedETag}");
            }

            return response;
        }

    }
}
