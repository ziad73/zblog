using Database;
using Entities;
using Microsoft.EntityFrameworkCore;
using Models.Like;
using Services.Like.Contracts;

namespace Services.Like;

public class LikeServices : ILikeServices
{
  private readonly ApplicationDbContext _context;

  public LikeServices(ApplicationDbContext context)
  {
    _context = context;
  }

  /// <summary>Likes a blog post or a comment. A user may like a given target at most once.(PostId XOR CommentId)</summary>
  /// <param name="userId">The ID of the authenticated user.</param>
  /// <param name="dto">Payload with exactly one of PostId or CommentId.</param>
  /// <returns>The created like details, or null if the target is invalid, not found, or already liked.</returns>
  public async Task<LikeResponseDto?> LikeAsync(Guid userId, LikeRequestDto dto)
  {
    // PostId XOR CommentId
    if ((dto.PostId is null) == (dto.CommentId is null))
      return null;

    // validate like a post
    if (dto.PostId.HasValue)
    {
      var post = await _context.blog_posts
        .Where(p => p.id == dto.PostId && p.is_deleted == false)
        .FirstOrDefaultAsync();

      if (post is null)
        return null;

      // already liked?
      var exists = await _context.likes
        .AnyAsync(l => l.user_id == userId && l.post_id == dto.PostId);

      if (exists)
        return null;
    }
    // validate like a comment
    else
    {
      var comment = await _context.comments
        .Where(c => c.id == dto.CommentId && c.is_deleted == false)
        .FirstOrDefaultAsync();

      if (comment is null)
        return null;

      // already liked?
      var exists = await _context.likes
        .AnyAsync(l => l.user_id == userId && l.comment_id == dto.CommentId);

      if (exists)
        return null;
    }

    var like = new Entities.Like
    {
      user_id = userId,
      post_id = dto.PostId,
      comment_id = dto.CommentId
    };

    // Add like to db
    _context.likes.Add(like);
    await _context.SaveChangesAsync();

    return new LikeResponseDto(
      like.id,
      like.user_id,
      like.post_id,
      like.comment_id,
      like.created_at
    );
  }

  /// <summary>Removes a like from a blog post or a comment (hard delete).</summary>
  /// <param name="userId">The ID of the authenticated user.</param>
  /// <param name="postId">The post GUID to unlike (null if unliking a comment).</param>
  /// <param name="commentId">The comment GUID to unlike (null if unliking a post).</param>
  /// <returns>True if removed, false if the like was not found or target is invalid.</returns>
  public async Task<bool> UnlikeAsync(Guid userId, Guid? postId, Guid? commentId)
  {
    if ((postId is null) == (commentId is null))
      return false;

    Entities.Like? like;
    if (postId.HasValue)
    {
      like = await _context.likes
        .FirstOrDefaultAsync(l => l.user_id == userId && l.post_id == postId);
    }
    else
    {
      like = await _context.likes
        .FirstOrDefaultAsync(l => l.user_id == userId && l.comment_id == commentId);
    }

    if (like is null)
      return false;

    _context.likes.Remove(like);
    await _context.SaveChangesAsync();

    return true;
  }
}
