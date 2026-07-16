namespace Models.Auth;

public record LoginResponseDto(
  Guid UserId,
  string Message,
  string Username,
  string Email,
  List<string> Roles,
  string Token,
  DateTime ExpiresAt
);
