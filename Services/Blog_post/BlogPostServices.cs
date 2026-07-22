using Database;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models.Blog_post;
using Models.Comment;
using Services.Blog_post.Contracts;

namespace Services.Blog_post;

public class BlogPostServices : IBlogPostServices
{
  private readonly ILogger<BlogPostServices> _logger;
  private readonly ApplicationDbContext _context;
  private readonly UserManager<User> _userManager;
  private readonly IMemoryCache _cache;
  public BlogPostServices(ILogger<BlogPostServices> logger, ApplicationDbContext context, UserManager<User> userManager, IMemoryCache cache)
  {
    _logger = logger;
    _context = context;
    _userManager = userManager;
    _cache = cache;
  }

  /// <summary>Creates a new blog post and returns the full detail view.</summary>
  /// <param name="userId">The ID of the authenticated author creating the post.</param>
  /// <param name="dto">Payload with title and content.</param>
  /// <returns>The created blog post with nested comments and counts.</returns>
  public async Task<BlogPostDetailResponseDto> CreateBlogPost(Guid userId, CreateBlogPostRequestDto dto)
  {
    var post = new blog_post
    {
      title = dto.Title,
      content = dto.Content,
      author_id = userId
    };

    _context.blog_posts.Add(post);
    await _context.SaveChangesAsync();

    // Manual invalidation
    _cache.Remove("blogpost:list");

    return (await GetBlogPostById(post.id))!;
  }

  /// <summary>Retrieves all non-deleted blog posts with author info, roles, and comment/like counts.</summary>
  /// <returns>A list of blog posts sorted by creation date.</returns>
  public async Task<List<BlogPostListResponseDto>> GetAllBlogPosts()
  {
    return (await _cache.GetOrCreateAsync("blogpost:list", async entry =>
    {
      // Time-based invalidation
      entry.SlidingExpiration = TimeSpan.FromMinutes(5); // entry expires if not accessed within a window
      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30); // entry expires after a fixed duration, no matter what
      entry.Size = 1;

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
    }))!;
  }

  /// <summary>Gets a single non-deleted blog post with its nested comment tree and like count.</summary>
  /// <param name="id">The post GUID.</param>
  /// <returns>The blog post with author info, nested comments, and counts, or null if not found or soft-deleted.</returns>
  public async Task<BlogPostDetailResponseDto?> GetBlogPostById(Guid id)
  {
    var cacheKey = $"blogpost:{id}";
    return await _cache.GetOrCreateAsync(cacheKey, async entry =>
    {
      entry.SlidingExpiration = TimeSpan.FromMinutes(5);
      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
      entry.Size = 1;

      var post = await _context.blog_posts
        .Include(p => p.author)
        .Where(p => p.id == id && p.is_deleted == false)
        .FirstOrDefaultAsync();

      if (post is null)
        return null;

      var roles = (await _userManager.GetRolesAsync(post.author)).ToList();

      var comments = await _context.comments
        .Include(c => c.author)
        .Include(c => c.likes)
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
          c.likes.Count,
          commentLookup[c.id].Count(),
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
    });
  }

  /// <summary>Updates a blog post's title and content. Only the post author or an admin may update.</summary>
  /// <param name="id">The post GUID.</param>
  /// <param name="userId">The ID of the requesting user.</param>
  /// <param name="dto">Payload with new title and content.</param>
  /// <returns>The updated blog post, or null if not found or not authorized.</returns>
  public async Task<BlogPostDetailResponseDto?> UpdateBlogPost(Guid id, Guid userId, UpdateBlogPostRequestDto dto)
  {
    var post = await _context.blog_posts
      .Where(p => p.id == id && p.is_deleted == false)
      .FirstOrDefaultAsync();

    if (post is null)
      return null;

    if (post.author_id != userId)
    {
      var requestingUser = await _userManager.FindByIdAsync(userId.ToString());
      if (requestingUser is null || !await _userManager.IsInRoleAsync(requestingUser, "admin"))
        return null;
    }

    post.title = dto.Title;
    post.content = dto.Content;
    post.updated_at = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    // Manual invalidation
    _cache.Remove("blogpost:list");
    _cache.Remove($"blogpost:{id}");

    return await GetBlogPostById(post.id);
  }

  /// <summary>Soft-deletes a blog post. Only the post author or an admin may delete.</summary>
  /// <param name="id">The post GUID.</param>
  /// <param name="userId">The ID of the requesting user.</param>
  /// <returns>True if deleted, false if not found or not authorized.</returns>
  public async Task<bool> SoftDeleteBlogPost(Guid id, Guid userId)
  {
    var post = await _context.blog_posts
      .Where(p => p.id == id && p.is_deleted == false)
      .FirstOrDefaultAsync();

    if (post is null)
      return false;

    if (post.author_id != userId)
    {
      var requestingUser = await _userManager.FindByIdAsync(userId.ToString());
      if (requestingUser is null || !await _userManager.IsInRoleAsync(requestingUser, "admin"))
        return false;
    }

    post.is_deleted = true;
    post.deleted_at = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    _cache.Remove("blogpost:list");
    _cache.Remove($"blogpost:{id}");

    return true;
  }
}
