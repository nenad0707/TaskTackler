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

            // Provera tipa metode i invalidacija keša za modifikacione operacije
            if (request.Method != HttpMethod.Get)
            {
                // Brisati ETag iz localStorage
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", $"etag-{uriKey}");
                Console.WriteLine("[CashingHandler] Cache cleared for modification request.");
            }

            // Provera da li postoji ETag u localStorage za GET zahteve
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
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag, false));
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
