using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskTackler.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IAuthService _authService;
    private bool _isPrerenderingDone = false;


    public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, IAuthService authService)
    {
        _jsRuntime = jsRuntime;
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        //if (!_isPrerenderingDone)
        //{
        //    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        //}
        //var authToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        //var identity = new ClaimsIdentity();
        //if (!string.IsNullOrEmpty(authToken))
        //{
        //    identity = new ClaimsIdentity(ParseClaimsFromJwt(authToken), "jwt");
        //}

        //var user = new ClaimsPrincipal(identity);
        //return new AuthenticationState(user);

        var authToken = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        var identity = new ClaimsIdentity();
        if (!string.IsNullOrEmpty(authToken))
        {
            var username = GetUsernameFromJwt(authToken);
            identity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Name, username)
    }, "jwt");
        }

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);

    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(jwt);
        return jsonToken.Claims;
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<string> GetCurrentUserUsernameAsync()
    {
        var authState = await GetAuthenticationStateAsync();
        var user = authState.User;
        if (user.Identity!.IsAuthenticated)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        }
        else
        {
            return "Gost";
        }
    }

    private string GetUsernameFromJwt(string jwt)
    {
        var claims = ParseClaimsFromJwt(jwt);
        var usernameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        if (usernameClaim != null)
        {
            return usernameClaim.Value;
        }
        return "Unknown"; 
    }
}
