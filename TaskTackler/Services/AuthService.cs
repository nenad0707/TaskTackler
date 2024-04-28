using Microsoft.JSInterop;
using System.Net.Http.Json;
using TaskTackler.Models;

namespace TaskTackler.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public AuthService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime)
    {
        _httpClient = httpClientFactory.CreateClient("api");
        _jsRuntime = jsRuntime;
    }

    public async Task<LoginResult> Login(LoginDTO loginModel)
    {
        var response = await _httpClient.PostAsJsonAsync("Auth/login", loginModel);
        if (response.IsSuccessStatusCode)
        {
            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenModel>();
            if (tokenResponse?.Token != null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", tokenResponse.Token);
                return new LoginResult { IsSuccess = true, Token = tokenResponse.Token };
            }
        }
        return new LoginResult { IsSuccess = false, ErrorMessage = "Login failed. Please check your username and password and try again." };
    }

    public async Task Logout()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string> GetTokenAsync()
    {
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        return token;
    }

    public async Task<RegisterResult> Register(RegisterDTO registerModel)
    {
        var response = await _httpClient.PostAsJsonAsync("Auth/register", registerModel);
        if (response.IsSuccessStatusCode)
        {
            return new RegisterResult { IsSuccess = true };
        }
        else
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            return new RegisterResult { IsSuccess = false, ErrorMessage = errorResponse };
        }
    }
}
