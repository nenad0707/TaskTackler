using TaskTackler.Models;

namespace TaskTackler.Services;

public interface IAuthService
{

    Task<LoginResult> Login(LoginDTO loginModel);
    Task Logout();
    Task<bool> IsAuthenticatedAsync();
    Task<string> GetTokenAsync();
    Task<RegisterResult> Register(RegisterDTO registerModel);
}
