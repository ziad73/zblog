using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Entities;
public class Comment
{
    public Guid id { get; set; } = Guid.NewGuid();
    public string content { get; set; }
    public bool is_deleted { get; set; } = false;
    public DateTime? deleted_at { get; set; }
    public DateTime created_at { get; set; } = DateTime.UtcNow;
    public DateTime updated_at { get; set; } = DateTime.UtcNow;

    // Self Relation: Nested comments
    // FK
    public Guid? parent_comment_id { get; set; }
    // Navigation properties
    public Comment? parent_comment { get; set; }
    public List<Comment> replies { get; set; } = new List<Comment>();// required but with no waraning 


    // Relation with blog_post
    // FK
    public Guid post_id { get; set; }
    // Navigation property
    public blog_post post { get; set; } = null!;

    // Relation with user
    // FK
    public Guid author_id { get; set; }
    // Navigation property
    public User author { get; set; }= null!;

    // Navigation property with like
    public List<Like> likes { get; set; } = new List<Like>();// required but with no waraning
}
