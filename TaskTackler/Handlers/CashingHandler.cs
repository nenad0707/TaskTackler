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
                    etag = etag.Trim('\"'); // Uklanjanje navodnika ako već postoje
                    request.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{etag}\"", false));
                    Console.WriteLine($"[CashingHandler] ETag from localStorage: \"{etag}\"");
                }
            }

            var response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine("[CashingHandler] Response received.");

            // Sačuvati novi ETag u localStorage ako je dostupan
            if (response.Headers.ETag != null && request.Method == HttpMethod.Get)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", $"etag-{uriKey}", response.Headers.ETag.Tag);
                Console.WriteLine($"[CashingHandler] ETag saved: {response.Headers.ETag.Tag}");
            }

            return response;
        }

    }
}
