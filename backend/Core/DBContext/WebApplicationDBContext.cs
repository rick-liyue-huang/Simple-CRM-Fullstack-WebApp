using backend.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Core.DBContext;

public class WebApplicationDBContext : IdentityDbContext<WebApplicationUser>
{
  public WebApplicationDBContext(DbContextOptions<WebApplicationDBContext> options) : base(options)
  {
  }

  public DbSet<Message> Messages { get; set; }
  public DbSet<Log> Logs { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // confirm the identity tables are created with the correct names
    modelBuilder.Entity<WebApplicationUser>(e =>
    {
      e.ToTable("Users");
    });
    modelBuilder.Entity<IdentityUserClaim<string>>(e =>
    {
      e.ToTable("UserClaims");
    });
    modelBuilder.Entity<IdentityUserLogin<string>>(e =>
    {
      e.ToTable("UserLogins");
    });
    modelBuilder.Entity<IdentityUserToken<string>>(e =>
    {
      e.ToTable("UserTokens");
    });
    modelBuilder.Entity<IdentityRole>(e =>
    {
      e.ToTable("Roles");
    });
    modelBuilder.Entity<IdentityRoleClaim<string>>(e =>
    {
      e.ToTable("RoleClaims");
    });
    modelBuilder.Entity<IdentityUserRole<string>>(e =>
    {
      e.ToTable("UserRoles");
    });



  }

}

