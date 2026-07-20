using Database;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.Comment;
using Services.Comment.Contracts;

namespace Services.Comment;

public class CommentServices : ICommentServices
{
  private readonly ILogger<CommentServices> _logger;
  private readonly ApplicationDbContext _context;
  private readonly UserManager<User> _userManager;
  public CommentServices(ILogger<CommentServices> logger, ApplicationDbContext context, UserManager<User> userManager)
  {
    _logger = logger;
    _context = context;
    _userManager = userManager;
  }

  public async Task<List<CommentListResponseDto>> GetAllComments()
  {
    var comments = await _context.comments
      .Where(c => c.is_deleted == false)
      .Select(c => new CommentListResponseDto(
        c.id,
        c.content ?? string.Empty,
        c.author_id,
        c.author.UserName ?? string.Empty,// no need for include with select, it generates the SQL JOIN automatically
        c.created_at,
        c.post_id,
        c.parent_comment_id,
        c.likes.Count
      ))
      .AsNoTracking()
      .ToListAsync();

    return comments;
  }

  public async Task<CommentResponseDto?> GetCommentById(Guid id)
  {
    var comment = await _context.comments
      .Include(c => c.author)
      .Where(c => c.id == id && c.is_deleted == false)
      .FirstOrDefaultAsync();

    if (comment is null)
      return null;

    var allPostComments = await _context.comments
      .Include(c => c.author)
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
        BuildTree(c.id)
      )).ToList();
    }

    return new CommentResponseDto(
      comment.id,
      comment.content ?? string.Empty,
      comment.author_id,
      comment.author.UserName ?? string.Empty,
      comment.created_at,
      BuildTree(comment.id)
    );
  }

  public async Task<CommentResponseDto> CreateComment(Guid userId, CreateCommentRequestDto dto)
  {
    throw new NotImplementedException();
  }

  public Task<CommentResponseDto?> UpdateComment(Guid id, Guid userId, UpdateCommentRequestDto dto)
  {
    throw new NotImplementedException();
  }

  public Task<bool> SoftDeleteComment(Guid id, Guid userId)
  {
    throw new NotImplementedException();
  }
}
