using backend.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Core.DBContext;

// create a new class that inherits from IdentityDbContext<ApplicationUser>, and add the DbSet properties for the Log and Message entities; also, override the OnModelCreating method to customize the tables when the ApplicationUser model is created.
public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
{
  public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
  {

  }

  public DbSet<Log> Logs { get; set; }

  public DbSet<Message> Messages { get; set; }

  // customized the tables when the ApplicationUser model is created.
  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.Entity<ApplicationUser>(u =>
    {
      u.ToTable("Users");
    });

    builder.Entity<IdentityUserClaim<string>>(c =>
    {
      c.ToTable("UserClaims");
    });

    builder.Entity<IdentityUserLogin<string>>(l =>
    {
      l.ToTable("UserLogins");
    });

    builder.Entity<IdentityUserToken<string>>(t =>
    {
      t.ToTable("UserTokens");
    });

    builder.Entity<IdentityRole>(r =>
    {
      r.ToTable("Roles");
    });

    builder.Entity<IdentityRoleClaim<string>>(c =>
    {
      c.ToTable("RoleClaims");
    });

    builder.Entity<IdentityUserRole<string>>(ur =>
    {
      ur.ToTable("UserRoles");
    });
  }
}
