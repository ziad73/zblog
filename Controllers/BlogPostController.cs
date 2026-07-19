
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
  [HttpPost]
  [Authorize(Policy = "RequireAuthor")]
  public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostRequestDto dto)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var result = await _blogPostServices.CreateBlogPost(userId, dto);
    return CreatedAtAction(nameof(GetBlogPostById), new { id = result.Id }, result);// 201 Created
  }

  [HttpPut("{id:guid}")]
  [Authorize(Policy = "RequireAuthor")]
  public async Task<IActionResult> UpdateBlogPost(Guid id, [FromBody] UpdateBlogPostRequestDto dto)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var result = await _blogPostServices.UpdateBlogPost(id, userId, dto);
    if (result is null)
    {
      var existing = await _blogPostServices.GetBlogPostById(id);
      if (existing is null)
        return NotFound(new { message = $"Blog post with id '{id}' not found." });
      return StatusCode(403, new { message = "You are not the owner of this post." });// 403 Forbidden
    }
    return Ok(result);
  }

  [HttpDelete("{id:guid}")]
  [Authorize(Policy = "RequireAuthor")]
  public async Task<IActionResult> SoftDeleteBlogPost(Guid id)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var deleted = await _blogPostServices.SoftDeleteBlogPost(id, userId);
    if (!deleted)
    {
      var existing = await _blogPostServices.GetBlogPostById(id);
      if (existing is null)
        return NotFound(new { message = $"Blog post with id '{id}' not found." });
      return StatusCode(403, new { message = "You are not the owner of this post." });
    }
    return NoContent();
  }

}
