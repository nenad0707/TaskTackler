using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace TaskTackler.Handlers;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _jsRuntime;

    public AuthorizationMessageHandler(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_jsRuntime is IJSInProcessRuntime) // Check if execution is on the client side
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
