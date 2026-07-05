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
  }
  
}
