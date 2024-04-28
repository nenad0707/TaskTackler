namespace TaskTackler.Models;

public class LoginResult
{
    public bool IsSuccess { get; set; }
    public string Token { get; set; } = string.Empty;   
    public string ErrorMessage { get; set; } = string.Empty;
}
