using Models.Blog_post;

namespace Services.Blog_post.Contracts;
public interface IBlogPostServices
{
  public Task<List<BlogPostListResponseDto>> GetAllBlogPosts();
  public Task<BlogPostDetailResponseDto?> GetBlogPostById(Guid id);
  public Task<BlogPostDetailResponseDto> CreateBlogPost(CreateBlogPostRequestDto blogPostRequestDto);
  public Task<BlogPostDetailResponseDto> UpdateBlogPost(UpdateBlogPostRequestDto blogPostRequestDto);
  public Task SoftDeleteBlogPost(Guid id);
}
