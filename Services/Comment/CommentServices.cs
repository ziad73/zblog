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
    throw new NotImplementedException();
  }

  public Task<CommentResponseDto> CreateComment(Guid userId, CreateCommentRequestDto dto)
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
