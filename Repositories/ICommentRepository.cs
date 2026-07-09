using Entities;

namespace Repositories
{
  public interface ICommentRepository
  {
    Task<Comment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Comment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Comment entity, CancellationToken cancellationToken = default);
    void Update(Comment entity);
    void Delete(Comment entity);

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
