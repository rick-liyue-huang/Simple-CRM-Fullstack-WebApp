using System.ComponentModel.DataAnnotations;

namespace backend.Core.Dtos.Auth;

public class RegisterDto
{
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Username is required")]
  public string UserName { get; set; } = string.Empty;

  [Required(ErrorMessage = "Password is required")]
  public string Password { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  public string Address { get; set; } = string.Empty;


}
