using Database;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.Blog_post;
using Models.Comment;
using Services.Blog_post.Contracts;

namespace Services.Blog_post;

public class BlogPostServices : IBlogPostServices
{
  private readonly ILogger<BlogPostServices> _logger;
  private readonly ApplicationDbContext _context;
  private readonly UserManager<User> _userManager;
  public BlogPostServices(ILogger<BlogPostServices> logger, ApplicationDbContext context, UserManager<User> userManager)
  {
    _logger = logger;
    _context = context;
    _userManager = userManager;
  }

  public Task<BlogPostDetailResponseDto> CreateBlogPost(CreateBlogPostRequestDto blogPostRequestDto)
  {
    // extract author id from jwt sub claim
    throw new NotImplementedException();
  }

  public async Task<List<BlogPostListResponseDto>> GetAllBlogPosts()
  {
    var posts = await _context.blog_posts
    .Where(p => p.is_deleted == false)
    .Select(p => new
    {
      p.id,
      p.title,
      p.content,
      p.created_at,
      p.updated_at,
      p.author_id,
      p.author.UserName,
      p.author.Email,
      CommentsCount = p.comments.Count,
      LikesCount = p.likes.Count
    })
    .AsNoTracking()
    .ToListAsync();

    var authorIds = posts.Select(p => p.author_id).Distinct();
    var rolesDict = await _context.Roles
      .Join(_context.UserRoles.Where(ur => authorIds.Contains(ur.UserId)),
        r => r.Id, ur => ur.RoleId, (r, ur) => new { ur.UserId, RoleName = r.Name })
      .GroupBy(x => x.UserId)
      .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.RoleName).ToList());

    return posts.Select(p => new BlogPostListResponseDto(
      p.id,
      p.title ?? string.Empty,
      p.content ?? string.Empty,
      p.created_at,
      p.updated_at,
      p.author_id,
      p.UserName ?? string.Empty,
      p.Email ?? string.Empty,
      rolesDict.GetValueOrDefault(p.author_id, new List<string>()),
      p.CommentsCount,
      p.LikesCount
    )).ToList();
  }

  public async Task<BlogPostDetailResponseDto?> GetBlogPostById(Guid id)
  {
    var post = await _context.blog_posts
      .Include(p => p.author)
      .Where(p => p.id == id && p.is_deleted == false)
      .FirstOrDefaultAsync();

    if (post is null)
      return null;

    var roles = (await _userManager.GetRolesAsync(post.author)).ToList();

    var comments = await _context.comments
      .Include(c => c.author)
      .Where(c => c.post_id == id && c.is_deleted == false)
      .ToListAsync();

    var likesCount = await _context.likes.CountAsync(l => l.post_id == id);

    var commentLookup = comments.ToLookup(c => c.parent_comment_id);

    List<CommentResponseDto> BuildTree(Guid? parentId)
    {
      return commentLookup[parentId].Select(c => new CommentResponseDto(
        c.id,
        c.content ?? string.Empty,
        c.author_id,
        c.author.UserName ?? string.Empty,
        c.created_at,
        BuildTree(c.id)
      )).ToList();
    }

    return new BlogPostDetailResponseDto(
      post.id,
      post.title ?? string.Empty,
      post.content ?? string.Empty,
      post.is_deleted,
      post.created_at,
      post.updated_at,
      post.author_id,
      post.author.UserName ?? string.Empty,
      post.author.Email ?? string.Empty,
      roles,
      comments.Count,
      likesCount,
      BuildTree(null)
    );
  }

  public async Task SoftDeleteBlogPost(Guid id)
  {
    var post = _context.blog_posts.Find(id);
    post?.is_deleted = true;
    post?.updated_at = DateTime.UtcNow;
    await _context.SaveChangesAsync();
    return;
  }

  public Task<BlogPostDetailResponseDto> UpdateBlogPost(UpdateBlogPostRequestDto blogPostRequestDto)
  {
    throw new NotImplementedException();
  }
}
