using Microsoft.AspNetCore.Http.HttpResults;

namespace Entities;
public class Like
{
    public Guid id { get; set; } = Guid.NewGuid();
    public DateTime created_at { get; set; } = DateTime.UtcNow;

    // Relation with user
    public Guid user_id { get; set; }
    public User user { get; set; } = null!;

    // Exactly one of these targets must be set.
    public Guid? comment_id { get; set; }
    public Comment? comment { get; set; }

    public Guid? post_id { get; set; }
    public blog_post? post { get; set; }
}
