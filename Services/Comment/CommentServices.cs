using Database;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models.Comment;
using Services.Comment.Contracts;

namespace Services.Comment;

public class CommentServices : ICommentServices
{
  private readonly ILogger<CommentServices> _logger;
  private readonly ApplicationDbContext _context;
  private readonly UserManager<User> _userManager;
  private readonly IMemoryCache _cache;
  public CommentServices(ILogger<CommentServices> logger, ApplicationDbContext context, UserManager<User> userManager, IMemoryCache cache)
  {
    _logger = logger;
    _context = context;
    _userManager = userManager;
    _cache = cache;
  }

  /// <summary>Retrieves all non-deleted comments for a given post as a flat list with author info and like counts.</summary>
  /// <param name="postId">The ID of the post to fetch comments for.</param>
  /// <returns>A list of <see cref="CommentListResponseDto"/> sorted by creation date.</returns>
  public async Task<List<CommentListResponseDto>> GetAllComments(Guid postId)
  {
    var cacheKey = $"comments:{postId}";
    return (await _cache.GetOrCreateAsync(cacheKey, async entry =>
    {
      entry.SlidingExpiration = TimeSpan.FromMinutes(5);
      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
      entry.Size = 1;

      var comments = await _context.comments
        .Where(c => c.post_id == postId && c.is_deleted == false)
        .OrderBy(c => c.created_at)
        .Select(c => new CommentListResponseDto(
          c.id,
          c.content ?? string.Empty,
          c.author_id,
          c.author.UserName ?? string.Empty,
          c.created_at,
          c.post_id,
          c.parent_comment_id,
          c.likes.Count,
          c.replies.Count(r => !r.is_deleted)
        ))
        .AsNoTracking()
        .ToListAsync();

      return comments;
    }))!;
  }

  /// <summary>Gets a single non-deleted comment with its nested reply tree.</summary>
  /// <param name="id">The comment GUID.</param>
  /// <returns>The comment with nested replies, or null if not found or soft-deleted.</returns>
  public async Task<CommentResponseDto?> GetCommentById(Guid id)
  {
    var cacheKey = $"comment:{id}";
    return await _cache.GetOrCreateAsync(cacheKey, async entry =>
    {
      entry.SlidingExpiration = TimeSpan.FromMinutes(5);
      entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
      entry.Size = 1;

      var comment = await _context.comments
        .Include(c => c.author)
        .Where(c => c.id == id && c.is_deleted == false)
        .FirstOrDefaultAsync();

      if (comment is null)
        return null;

      var allPostComments = await _context.comments
        .Include(c => c.author)
        .Include(c => c.likes)
        .Where(c => c.post_id == comment.post_id && c.is_deleted == false)
        .ToListAsync();

      var commentLookup = allPostComments.ToLookup(c => c.parent_comment_id);

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

      return new CommentResponseDto(
        comment.id,
        comment.content ?? string.Empty,
        comment.author_id,
        comment.author.UserName ?? string.Empty,
        comment.created_at,
        comment.likes.Count,
        commentLookup[comment.id].Count(),
        BuildTree(comment.id)
      );
    });
  }

  /// <summary>Creates a new comment on a post or as a reply to another comment.</summary>
  /// <param name="userId">The ID of the authenticated user creating the comment.</param>
  /// <param name="dto">The comment payload containing content, post ID, and optional parent comment ID.</param>
  /// <returns>The created comment with its nested reply subtree, or null if the post/parent comment is invalid.</returns>
  public async Task<CommentResponseDto?> CreateComment(Guid userId, CreateCommentRequestDto dto)
  {
    // validate post
    var post = await _context.blog_posts
      .Where(p => p.id == dto.PostId && p.is_deleted == false)
      .FirstOrDefaultAsync();

    if (post is null)
      return null;

    if (dto.ParentCommentId.HasValue)
    {
      var parent = await _context.comments
        .Where(c => c.id == dto.ParentCommentId && c.is_deleted == false)
        .FirstOrDefaultAsync();

      if (parent is null || parent.post_id != dto.PostId)
        return null;
    }

    // create comment
    var comment = new Entities.Comment
    {
      content = dto.Content,
      post_id = dto.PostId,
      parent_comment_id = dto.ParentCommentId,
      author_id = userId
    };
    // add comment to DB
    _context.comments.Add(comment);
    await _context.SaveChangesAsync();

    _cache.Remove($"comments:{dto.PostId}");
    _cache.Remove($"blogpost:{dto.PostId}");

    return await GetCommentById(comment.id);
  }

  /// <summary>Updates a comment's content. Only the comment author or an admin may update.</summary>
  /// <param name="id">The comment GUID.</param>
  /// <param name="userId">The ID of the requesting user.</param>
  /// <param name="dto">The updated content.</param>
  /// <returns>The updated comment with its nested reply tree, or null if not found or not authorized.</returns>
  public async Task<CommentResponseDto?> UpdateComment(Guid id, Guid userId, UpdateCommentRequestDto dto)
  {
    // find comment
    var comment = await _context.comments
      .Where(c => c.id == id && c.is_deleted == false)
      .FirstOrDefaultAsync();

    if (comment is null)
      return null;
    // Ownership check: is it owner or admin?
    if (comment.author_id != userId)
    {
      var requestingUser = await _userManager.FindByIdAsync(userId.ToString());
      //  check IsInRoleAsync("admin") → not owner & not admin → return null
      if (requestingUser is null || !await _userManager.IsInRoleAsync(requestingUser, "admin"))
        return null;
    }

    // update comment
    comment.content = dto.Content;
    comment.updated_at = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    _cache.Remove($"comments:{comment.post_id}");
    _cache.Remove($"comment:{id}");
    _cache.Remove($"blogpost:{comment.post_id}");

    return await GetCommentById(comment.id);
  }

  /// <summary>Soft-deletes a comment. Only the comment author or an admin may delete.</summary>
  /// <param name="id">The comment GUID.</param>
  /// <param name="userId">The ID of the requesting user.</param>
  /// <returns>True if deleted, false if not found or not authorized.</returns>
  public async Task<bool> SoftDeleteComment(Guid id, Guid userId)
  {
    // find comment
    var comment = await _context.comments
      .Where(c => c.id == id && c.is_deleted == false)
      .FirstOrDefaultAsync();
    if (comment is null)
      return false;

    // Ownership check: is it owner or admin?
    if (comment.author_id != userId)
    {
      var requestingUser = await _userManager.FindByIdAsync(userId.ToString());
      //  check IsInRoleAsync("admin") → not owner & not admin → return false
      if (requestingUser is null || !await _userManager.IsInRoleAsync(requestingUser, "admin"))
        return false;
    }

    comment.is_deleted = true;
    comment.deleted_at = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    _cache.Remove($"comments:{comment.post_id}");
    _cache.Remove($"comment:{id}");
    _cache.Remove($"blogpost:{comment.post_id}");

    return true;
  }
}
