using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskTackler.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IAuthService _authService;

    public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, IAuthService authService)
    {
        _jsRuntime = jsRuntime;
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var authToken = await _authService.GetTokenAsync();
        ClaimsIdentity identity = new ClaimsIdentity();

        if (!string.IsNullOrEmpty(authToken))
        {
            var claims = ParseClaimsFromJwt(authToken);
            var usernameClaim = claims.FirstOrDefault(c => c.Type == "unique_name");
            if (usernameClaim != null)
            {
                identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, usernameClaim.Value)
                }, "jwt");
            }
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
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _authService.Logout();
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }
}
