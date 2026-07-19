namespace Models.Comment;

public record CommentResponseDto(
  Guid Id,
  string Content,
  Guid AuthorId,
  string AuthorUsername,
  DateTime CreatedAt,
  List<CommentResponseDto>? Replies
);
