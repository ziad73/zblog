namespace Models.Like;

public record LikeRequestDto(
  Guid? PostId,
  Guid? CommentId
);

public record LikeResponseDto(
  Guid Id,
  Guid UserId,
  Guid? PostId,
  Guid? CommentId,
  DateTime CreatedAt
);
