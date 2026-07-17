namespace Models.Comment;

public record CommentResponseDto(
  Guid Id,
  string Content,
  Guid AuthorId,
  string AuthorName,
  DateTime CreatedAt,
  List<CommentResponseDto>? Replies
);
