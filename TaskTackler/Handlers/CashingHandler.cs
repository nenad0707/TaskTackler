using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using TaskTackler.Services;

namespace TaskTackler.Handlers
{
    public class CashingHandler : DelegatingHandler
    {
        private readonly CacheManager _cacheManager;

        public CashingHandler(CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string uriKey = request.RequestUri!.ToString();
            Console.WriteLine("[CashingHandler] Sending request.");

            // Brisanje keširanih podataka za POST, PUT i DELETE zahteve
            if (request.Method != HttpMethod.Get)
            {
                await _cacheManager.ClearSpecificItemsAsync(uriKey);
                Console.WriteLine("[CashingHandler] Specific ETags and data cleared.");
            }

            // Dodavanje ETag zaglavlja za GET zahteve
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

            // Čuvanje ETag-a i podataka za uspešne GET zahteve
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

            // Ako je odgovor 304, vraćanje keširanih podataka
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
    }

}
