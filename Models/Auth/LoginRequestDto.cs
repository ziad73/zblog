using System.ComponentModel.DataAnnotations;
using Entities;

namespace Models.Auth;

public record LoginRequestDto
{
  [Required]
  [StringLength(20, MinimumLength = 3)]
  public string Username { get; init; } = default!;

  [Required]
  [MinLength(6)]
  [DataType(DataType.Password)]
  public string Password { get; init; } = default!;
}
