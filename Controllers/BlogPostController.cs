
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
  //  PUT	/api/blogpost/{id}	Update a blog post (owner or Admin)	Authorized
  //  DELETE	/api/blogpost/{id}	Soft delete a blog post (owner or Admin)	Authorized

}
