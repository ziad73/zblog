using Microsoft.AspNetCore.Http.HttpResults;

namespace Entities;
public class Like
{
    public Guid id { get; set; } = Guid.NewGuid();
    public DateTime created_at { get; set; } = DateTime.UtcNow;

    // Relation with user
    public Guid user_id { get; set; }
    public User user { get; set; } = null!;

    // Relation with comment
    public Guid comment_id { get; set; }
    public Comment comment { get; set; } = null!;
    // Relation with post
    public Guid post_id { get; set; }
    public blog_post post { get; set; } = null!;
}
