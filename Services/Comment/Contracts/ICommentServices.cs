using Models.Comment;

namespace Services.Comment.Contracts;
public interface ICommentServices
{
  Task<List<CommentListResponseDto>> GetAllComments();
  Task<CommentResponseDto?> GetCommentById(Guid id);
  Task<CommentResponseDto> CreateComment(Guid userId, CreateCommentRequestDto dto);
  Task<CommentResponseDto?> UpdateComment(Guid id, Guid userId, UpdateCommentRequestDto dto);
  Task<bool> SoftDeleteComment(Guid id, Guid userId);
}
