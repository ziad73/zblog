using Models.Comment;

namespace Models.Blog_post;

public record CreateBlogPostRequestDto(
  string Title,
  string Content
);

public record UpdateBlogPostRequestDto(
  string Title,
  string Content
);

public record BlogPostListResponseDto(
  Guid Id,
  string Title,
  string Content,
  DateTime CreatedAt,
  DateTime UpdatedAt,
  Guid AuthorId,
  string AuthorUsername,
  string AuthorEmail,
  List<string> AuthorRole,
  int CommentsCount,
  int LikesCount
);

public record BlogPostDetailResponseDto(
  Guid Id,
  string Title,
  string Content,
  bool IsDeleted,
  DateTime CreatedAt,
  DateTime UpdatedAt,
  Guid AuthorId,
  string AuthorName,
  string AuthorEmail,
  string AuthorRole,
  int CommentsCount,
  int LikesCount,
  List<CommentResponseDto> Comments
);
