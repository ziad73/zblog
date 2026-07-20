using Models.Like;

namespace Services.Like.Contracts;

public interface ILikeServices
{
  Task<LikeResponseDto?> LikeAsync(Guid userId, LikeRequestDto dto);
  Task<bool> UnlikeAsync(Guid userId, Guid? postId, Guid? commentId);
}
