using System.ComponentModel.DataAnnotations;
using Entities;

namespace Models.Auth;

public record RegisterRequestDto
{
  [Required]
  public string Name { get; init; } = default!;

  [Required]
  [StringLength(20, MinimumLength = 3)]
  public string Username { get; init; } = default!;

  [Required]
  [EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
  public string Email { get; init; } = default!;

  [DataType(DataType.PhoneNumber)]
  public string Phone { get; init; } = default!;

  [Required]
  [MinLength(6)]
  [DataType(DataType.Password)]
  public string Password { get; init; } = default!;

  [Required]
  [MinLength(6)]
  [DataType(DataType.Password)]
  [Compare(nameof(Password), ErrorMessage = "Password and Confirm Password must match")]
  public string ConfirmPassword { get; init; } = default!;

}
