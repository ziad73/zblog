namespace Models.Comment;

public record CreateCommentRequestDto(
  string Content,
  Guid PostId,
  Guid? ParentCommentId
);

public record UpdateCommentRequestDto(
  string Content
);

public record CommentListResponseDto(
  Guid Id,
  string Content,
  Guid AuthorId,
  string AuthorUsername,
  DateTime CreatedAt,
  Guid PostId,
  Guid? ParentCommentId,
  int LikesCount,
  int RepliesCount
);

public record CommentResponseDto(
  Guid Id,
  string Content,
  Guid AuthorId,
  string AuthorUsername,
  DateTime CreatedAt,
  int LikesCount,
  int RepliesCount,
  List<CommentResponseDto>? Replies
);
