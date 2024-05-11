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
            var cacheControl = new CacheControlHeaderValue()
            {
                NoCache = true

            };
            request.Headers.CacheControl = cacheControl;

            string uriKey = request.RequestUri!.ToString();

            var etag = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", $"etag-{uriKey}");

            if (!string.IsNullOrEmpty(etag))
            {
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{etag}\"", true));
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.ETag != null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"etag-{uriKey}", response.Headers.ETag.Tag);
            }

            return response;
        }
    }
}
