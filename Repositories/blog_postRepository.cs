using Database;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
  public class blog_postRepository : Iblog_postRepository
  {
    private readonly ZBlogDbContext _dbContext;
    public blog_postRepository(ZBlogDbContext dbContext)
    {
      _dbContext = dbContext;
    }

    public async Task AddAsync(blog_post entity, CancellationToken cancellationToken = default)
    {
      await _dbContext.blog_posts.AddAsync(entity, cancellationToken);
    }

    public void Delete(blog_post entity)
    {
      _dbContext.blog_posts.Remove(entity);
    }

    public async Task<List<blog_post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
      return await _dbContext.blog_posts.ToListAsync(cancellationToken);
    }

    public Task<blog_post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return _dbContext.blog_posts.FindAsync([id], cancellationToken).AsTask();
    }

    public void Update(blog_post entity)
    {
      _dbContext.blog_posts.Update(entity);
    }
  }
}
