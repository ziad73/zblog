using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Entities;
public class blog_post
{
  public Guid id { get; set; } = Guid.NewGuid();
  [MaxLength(255)]
  public string title { get; set; }
  public string content { get; set; }
  public bool is_deleted { get; set; } = false;
  public DateTime? deleted_at { get; set; }
  public DateTime created_at { get; set; } = DateTime.UtcNow;
  public DateTime updated_at { get; set; } = DateTime.UtcNow;

  // Navigation property 
  // For comments
  public List<Comment> comments { get; set; } = new List<Comment>();
  // For Likes
  public List<Like> likes { get; set; } = new List<Like>();

  // Relation with author
  // FK
  public Guid author_id { get; set; }
  // Navigation property
  public User author { get; set; }= null!; // required but with no waraning
}
