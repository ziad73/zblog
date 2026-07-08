using Entities;

namespace Repositories
{
  public interface ILikeRepository : IRepository<Like>
  {
  // ILikeRepository
  // duplicate-like checks
  // like/unlike by post
  // like/unlike by comment

  // ExistsForPostAsync(Guid userId, Guid postId)
  // ExistsForCommentAsync(Guid userId, Guid commentId)
  // AddPostLikeAsync(Guid userId, Guid postId)
  // AddCommentLikeAsync(Guid userId, Guid commentId)
  // RemovePostLikeAsync(Guid userId, Guid postId)
  // RemoveCommentLikeAsync(Guid userId, Guid commentId)
  }
}
