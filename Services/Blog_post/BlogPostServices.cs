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

  public async Task<BlogPostDetailResponseDto> GetBlogPostById(Guid id)
  {
    throw new NotImplementedException();
  }

  public Task SoftDeleteBlogPost(Guid id)
  {
    throw new NotImplementedException();
  }

  public Task<BlogPostDetailResponseDto> UpdateBlogPost(UpdateBlogPostRequestDto blogPostRequestDto)
  {
    throw new NotImplementedException();
  }
}
