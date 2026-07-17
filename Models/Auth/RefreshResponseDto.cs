namespace Models.Auth;

public record RefreshResponseDto(
  Guid UserId,
  string Email,
  List<string> Roles,
  string AccessToken,
  DateTime ExpiresAt,
  string RefreshToken,
  DateTime RefreshExpiresAt
);
