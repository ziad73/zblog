using Entities;

namespace Repositories
{
  public interface Iblog_postRepository : IRepository<blog_post>
  {
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
