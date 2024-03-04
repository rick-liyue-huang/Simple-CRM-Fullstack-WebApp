
namespace backend.Core.Dtos.Auth;

public class UserInfoResult
{
  public string Id { get; set; }

  public string FirstName { get; set; } = string.Empty;

  public string LastName { get; set; } = string.Empty;

  public string UserName { get; set; } = string.Empty;

  public string Email { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; }

  public IEnumerable<string> Roles { get; set; }

}
