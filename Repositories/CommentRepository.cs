using Database;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
  public class CommentRepository : ICommentRepository
  {
    private readonly ZBlogDbContext _dbContext;

    public CommentRepository(ZBlogDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public Task AddAsync(Comment entity, CancellationToken cancellationToken = default)
    {
      return _dbContext.comments.AddAsync(entity, cancellationToken).AsTask();
    }

    public void Delete(Comment entity)
    {
      _dbContext.comments.Remove(entity);
    }

    public Task<List<Comment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
      return _dbContext.comments.ToListAsync(cancellationToken);
    }

    public Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return _dbContext.comments.FindAsync([id], cancellationToken).AsTask();
    }

    public void Update(Comment entity)
    {
      _dbContext.comments.Update(entity);
    }
  }
}
