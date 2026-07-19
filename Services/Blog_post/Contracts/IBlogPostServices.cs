using Models.Blog_post;

namespace Services.Blog_post.Contracts;
public interface IBlogPostServices
{
  public Task<List<BlogPostListResponseDto>> GetAllBlogPosts();
  public Task<BlogPostDetailResponseDto?> GetBlogPostById(Guid id);
  public Task<BlogPostDetailResponseDto> CreateBlogPost(Guid userId, CreateBlogPostRequestDto dto);
  public Task<BlogPostDetailResponseDto?> UpdateBlogPost(Guid id, Guid userId, UpdateBlogPostRequestDto dto);
  public Task<bool> SoftDeleteBlogPost(Guid id, Guid userId);
}
