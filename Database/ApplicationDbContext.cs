using Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Database;
public class ApplicationDbContext : IdentityDbContext<User, user_role, Guid>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
  {
  }

  // Define your DbSets here, for example:
  // public DbSet<YourEntity> YourEntities { get; set; }

  //TODO: Add your DbSets for the entities in your application
  // public DbSet<user> users { get; set; }
  // public DbSet<user_role> user_roles { get; set; }
  public DbSet<blog_post> blog_posts { get; set; }
  public DbSet<Comment> comments { get; set; }
  public DbSet<Like> likes { get; set; }

}
