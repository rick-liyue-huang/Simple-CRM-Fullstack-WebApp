using System.ComponentModel.DataAnnotations;

namespace backend.Core.Dtos.Auth;

public class UpdateRoleDto
{
  [Required(ErrorMessage = "Username is required")]
  public string UserName { get; set; } = string.Empty;

  public RoleType NewRole { get; set; }

}


public enum RoleType
{
  ADMIN,
  MANAGER,
  USER
  // OWNER, // this only can be changed from database, and other three can be changed from the client or APIs.
}
