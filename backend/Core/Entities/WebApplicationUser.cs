using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace backend.Core.Entities;

public class WebApplicationUser : IdentityUser
{
  public string FirstName { get; set; }

  public string LastName { get; set; }

  public string Address { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.Now;

  [NotMapped]
  public IList<string> Roles { get; set; }




}
