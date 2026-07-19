
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

  /// <summary>Get all blog posts (excludes soft-deleted).</summary>
  /// <response code="200">List of blog posts with author and counts.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not a member/author/admin.</response>
  [HttpGet]
  [Authorize(Policy = "RequireMember")]
  public async Task<IActionResult> GetAllBlogPosts()
  {
    var Result = await _blogPostServices.GetAllBlogPosts();
    return Ok(Result);
  }

  /// <summary>Get a single blog post by ID.</summary>
  /// <param name="id">The post GUID.</param>
  /// <response code="200">Blog post with comments and likes.</response>
  /// <response code="404">Post not found or soft-deleted.</response>
  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetBlogPostById(Guid id)
  {
    var result = await _blogPostServices.GetBlogPostById(id);
    if (result is null)
      return NotFound(new { message = $"Blog post with id '{id}' not found." });
    return Ok(result);
  }

  /// <summary>Create a new blog post.</summary>
  /// <param name="dto">Title and content.</param>
  /// <response code="201">Post created. Returns the full detail DTO.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not an author or admin.</response>
  [HttpPost]
  [Authorize(Policy = "RequireAuthor")]
  public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostRequestDto dto)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var result = await _blogPostServices.CreateBlogPost(userId, dto);
    return CreatedAtAction(nameof(GetBlogPostById), new { id = result.Id }, result);
  }

  /// <summary>Update a blog post (owner or admin only).</summary>
  /// <param name="id">The post GUID.</param>
  /// <param name="dto">New title and content.</param>
  /// <response code="200">Post updated. Returns the full detail DTO.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not the owner or admin.</response>
  /// <response code="404">Post not found or soft-deleted.</response>
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
      return StatusCode(403, new { message = "You are not the owner of this post." });
    }
    return Ok(result);
  }

  /// <summary>Soft-delete a blog post (owner or admin only).</summary>
  /// <param name="id">The post GUID.</param>
  /// <response code="204">Post soft-deleted successfully.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not the owner or admin.</response>
  /// <response code="404">Post not found or already soft-deleted.</response>
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
