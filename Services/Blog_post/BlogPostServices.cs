using Database;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models.Blog_post;
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
    return await _context.blog_posts
    .Where(p => p.is_deleted == false)
    .Select(p=> new BlogPostListResponseDto(
      p.id,
      p.title ?? string.Empty,
      p.content ?? string.Empty,
      p.created_at,
      p.updated_at,
      p.author_id,
      p.author.UserName?? string.Empty,
      p.author.Email ?? string.Empty,
      _userManager.GetRolesAsync(p.author).Result.ToList(),
      p.comments.Count,
      p.likes.Count
    ))
    .AsNoTracking() // Read-only optimization, disable tracking
    .ToListAsync();
  }

  public Task<BlogPostDetailResponseDto> GetBlogPostById(Guid id)
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
