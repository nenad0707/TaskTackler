using System.ComponentModel.DataAnnotations;

namespace TaskTackler.Models;


/// <summary>
/// Represents a data transfer object for user registration.
/// </summary>
public class RegisterDTO
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, ErrorMessage = "Username must be between 5 and 100 characters", MinimumLength = 5)]
    public string? Username { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be between 5 and 100 characters", MinimumLength = 5)]
    public string? Password { get; set; }
}
