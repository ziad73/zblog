using System.ComponentModel.DataAnnotations;

namespace Models.Auth;

public record RevokeRequest
{
  [Required]
  public string RefreshToken { get; init; } = default!;
}
