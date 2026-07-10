using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Entities;
public class User: IdentityUser<Guid>
{
  // Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, 
  // SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
  // LockoutEnd, LockoutEnabled, AccessFailedCount
  public string? Name { get; set; }
  public DateTime created_at { get; set; } = DateTime.UtcNow;
  public DateTime updated_at { get; set; } = DateTime.UtcNow;
}
