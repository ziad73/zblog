using Entities;
using Microsoft.EntityFrameworkCore;

namespace Database
{
  public class ZBlogDbContext : DbContext
  {
    public ZBlogDbContext(DbContextOptions<ZBlogDbContext> options) : base(options)
    {
    }

    // Define your DbSets here, for example:
    // public DbSet<YourEntity> YourEntities { get; set; }


    //TODO: Add your DbSets for the entities in your application
    public DbSet<blog_post> blog_posts { get; set; }
    public DbSet<Comment> comments { get; set; }
    public DbSet<Like> likes { get; set; }

  }
}
