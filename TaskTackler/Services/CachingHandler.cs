using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace TaskTackler.Services
{
    public class CachingHandler : DelegatingHandler
    {
        private readonly IJSRuntime _jsRuntime;

        public CachingHandler(IJSRuntime jsRuntime)
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

            var etag = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", $"etag-{request.RequestUri}");

            if (!string.IsNullOrEmpty(etag))
            {
                request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag));
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.ETag != null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"etag-{request.RequestUri}", response.Headers.ETag.Tag);
            }

            return response;
        }
    }
}
