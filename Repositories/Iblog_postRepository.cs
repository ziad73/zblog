using Entities;

namespace Repositories
{
  public interface Iblog_postRepository
  {
    Task<blog_post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<blog_post>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(blog_post entity, CancellationToken cancellationToken = default);
    void Update(blog_post entity);
    void Delete(blog_post entity);

  // Iblog_postRepository
  // soft delete
  // active list
  // author-based lookup
  
  // GetAllActiveAsync() or GetAllNotDeletedAsync()
  // GetByIdWithDetailsAsync(Guid id) if you want author/comments/likes included
  // GetByAuthorAsync(Guid authorId)
  // GetLatestAsync(int count)
  // SoftDeleteAsync(Guid id)
  // UpdateContentAsync(...) if you want a focused update path
  }
}
