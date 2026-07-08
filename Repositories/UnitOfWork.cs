using Database;

namespace Repositories
{
  public class UnitOfWork : IUnitOfWork
  {
    private readonly ZBlogDbContext _dbContext;

    public UnitOfWork(ZBlogDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
      return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
  }
}
