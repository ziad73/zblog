
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Blog_post;
using Services.Blog_post.Contracts;

namespace Controllers;
[ApiController]
[Route("api/blogpost")]
public class BlogPostController : ControllerBase
{
  private readonly IBlogPostServices _blogPostServices;
  public BlogPostController(IBlogPostServices blogPostServices)
  {
    _blogPostServices = blogPostServices;
  }

  /// <summary>
  /// Get all blog posts, excludes soft-deleted
  /// author + counts only
  /// </summary>
  /// <returns></returns>
  //  GET	/api/blogpost	List all blog posts (excludes soft-deleted)	Public
  [HttpGet]
  [Authorize(Policy="RequireMember")]
  public async Task<IActionResult> GetAllBlogPosts()
  {
    var Result = await _blogPostServices.GetAllBlogPosts();
    return Ok(Result);
  }
  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetBlogPostById(Guid id)
  {
    var result = await _blogPostServices.GetBlogPostById(id);
    if (result is null)
      return NotFound(new { message = $"Blog post with id '{id}' not found." });
    return Ok(result);
  }
  //  POST	/api/blogpost	Create a new blog post	Authorized
  // [HttpPost("create")]
  // [Authorize(Policy="RequireAuthor")]
  // public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostRequestDto blogPostRequestDto)
  // {
  //   var result = await _blogPostServices.CreateBlogPost(blogPostRequestDto);
  //   return Created(result);
  // }
  //  PUT	/api/blogpost/{id}	Update a blog post (owner or Admin)	Authorized
  
  //  DELETE	/api/blogpost/{id}	Soft delete a blog post (owner or Admin)	Authorized
  [HttpDelete("{id:guid}")]
  [Authorize(Policy="RequireAuthor")]
  public async Task<IActionResult> SoftDeleteBlogPost(Guid id)
  {
    await _blogPostServices.SoftDeleteBlogPost(id);
    return NoContent();
  }

}
