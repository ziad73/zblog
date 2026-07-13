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
  
  // relationship: 
  // with blog_post table
  public List<blog_post> posts { get; set; } = new List<blog_post>();
  // with comment table
  public List<Comment> comments { get; set; } = new List<Comment>();
  // with like table
  public List<Like> likes { get; set; } = new List<Like>();

}
