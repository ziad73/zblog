using Entities;

namespace Repositories
{
  public interface ICommentRepository : IRepository<Comment>
  {
  // ICommentRepository
  // post-thread retrieval
  // soft delete
  // parent/reply lookup

  // GetAllActiveAsync()
  // GetByPostIdAsync(Guid postId)
  // GetThreadAsync(Guid postId) or GetNestedCommentsAsync(Guid postId)
  // GetByParentCommentIdAsync(Guid? parentCommentId)
  // SoftDeleteAsync(Guid id)
  // GetByAuthorAsync(Guid authorId)
  }
}
