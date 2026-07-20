using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Comment;
using Services.Comment.Contracts;

namespace Controllers;

[ApiController]
[Route("api/comments")]
public class CommentController : ControllerBase
{
  private readonly ICommentServices _commentServices;

  public CommentController(ICommentServices commentServices)
  {
    _commentServices = commentServices;
  }

  /// <summary>List all comments for a given post (excludes soft-deleted).</summary>
  /// <param name="postId">The post GUID to fetch comments for.</param>
  /// <response code="200">List of comments with author info and like counts.</response>
  /// <response code="400">Missing or invalid postId.</response>
  [HttpGet]
  public async Task<IActionResult> GetAllComments([FromQuery] Guid postId)
  {
    var result = await _commentServices.GetAllComments(postId);
    return Ok(result);
  }

  /// <summary>Get a single comment by ID with its nested replies.</summary>
  /// <param name="id">The comment GUID.</param>
  /// <response code="200">Comment with nested reply tree.</response>
  /// <response code="404">Comment not found or soft-deleted.</response>
  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetCommentById(Guid id)
  {
    var result = await _commentServices.GetCommentById(id);
    if (result is null)
      return NotFound(new { message = $"Comment with id '{id}' not found." });
    return Ok(result);
  }

  /// <summary>Create a comment on a post or as a reply to another comment.</summary>
  /// <param name="dto">Payload with content, post ID, and optional parent comment ID.</param>
  /// <response code="201">Comment created. Returns the comment with nested replies.</response>
  /// <response code="400">Invalid payload or the post/parent comment does not exist.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not a member/author/admin.</response>
  [HttpPost]
  [Authorize(Policy = "RequireMember")]
  public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequestDto dto)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var result = await _commentServices.CreateComment(userId, dto);
    return CreatedAtAction(nameof(GetCommentById), new { id = result?.Id }, result);
  }

  /// <summary>Update a comment's content (owner or admin only).</summary>
  /// <param name="id">The comment GUID.</param>
  /// <param name="dto">Payload with new content.</param>
  /// <response code="200">Comment updated. Returns the comment with nested replies.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not the owner or admin.</response>
  /// <response code="404">Comment not found or soft-deleted.</response>
  [HttpPut("{id:guid}")]
  [Authorize(Policy = "RequireMember")]
  public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentRequestDto dto)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var result = await _commentServices.UpdateComment(id, userId, dto);
    if (result is null)
    {
      var existing = await _commentServices.GetCommentById(id);
      if (existing is null)
        return NotFound(new { message = $"Comment with id '{id}' not found." });
      return StatusCode(403, new { message = "You are not the owner of this comment." });
    }
    return Ok(result);
  }

  /// <summary>Soft-delete a comment (owner or admin only).</summary>
  /// <param name="id">The comment GUID.</param>
  /// <response code="204">Comment soft-deleted successfully.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not the owner or admin.</response>
  /// <response code="404">Comment not found or already soft-deleted.</response>
  [HttpDelete("{id:guid}")]
  [Authorize(Policy = "RequireMember")]
  public async Task<IActionResult> SoftDeleteComment(Guid id)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var deleted = await _commentServices.SoftDeleteComment(id, userId);
    if (!deleted)
    {
      var existing = await _commentServices.GetCommentById(id);
      if (existing is null)
        return NotFound(new { message = $"Comment with id '{id}' not found." });
      return StatusCode(403, new { message = "You are not the owner of this comment." });
    }
    return NoContent();
  }
}
