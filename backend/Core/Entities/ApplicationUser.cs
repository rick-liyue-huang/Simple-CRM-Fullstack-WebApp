using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace backend.Core.Entities;

public class ApplicationUser : IdentityUser
{
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;

  public string Address { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.Now;

  [NotMapped]
  public IList<string> Roles { get; set; }
}
