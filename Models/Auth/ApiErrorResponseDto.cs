namespace Models.Auth;

public record ApiErrorResponseDto(
  string Message,
  IEnumerable<string> Errors
);
