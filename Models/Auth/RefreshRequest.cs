using System.ComponentModel.DataAnnotations;

namespace Models.Auth;

public record RefreshRequest
{
  [Required]
  public string RefreshToken { get; init; } = default!;
}
