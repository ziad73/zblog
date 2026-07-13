namespace Models.Common;

public record ApiErrorResponseDto(
  string Message,
  IEnumerable<string> Errors
);
