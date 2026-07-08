using Database;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
  public class LikeRepository : ILikeRepository
  {
    private readonly ZBlogDbContext _dbContext;

    public LikeRepository(ZBlogDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public Task AddAsync(Like entity, CancellationToken cancellationToken = default)
    {
      return _dbContext.likes.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Delete(Like entity)
    {
      _dbContext.likes.Remove(entity);
    }

    public Task<List<Like>> GetAllAsync(CancellationToken cancellationToken = default)
    {
      return _dbContext.likes.ToListAsync(cancellationToken);
    }

    public Task<Like?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return _dbContext.likes.FindAsync([id], cancellationToken).AsTask();
    }

    public void Update(Like entity)
    {
      _dbContext.likes.Update(entity);
    }

  }
}
