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

  [HttpGet]
  public async Task<IActionResult> GetAllComments()
  {
    var result = await _commentServices.GetAllComments();
    return Ok(result);
  }

  [HttpGet("{id:guid}")]
  // GET	/api/comments/{id}	Get a single comment	Public
  public async Task<IActionResult> GetCommentById(Guid id)
  {
    var result = await _commentServices.GetCommentById(id);
    if (result is null)
      return NotFound(new { message = $"Comment with id '{id}' not found." });
    return Ok(result);
  }

  [HttpPost]
  [Authorize(Policy = "RequireMember")]
  // POST	/api/comments	Create a comment on a post or another comment	Authorized
  public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequestDto dto)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var result = await _commentServices.CreateComment(userId, dto);
    return CreatedAtAction(nameof(GetCommentById), new { id = result.Id }, result);
  }

  [HttpPut("{id:guid}")]
  [Authorize(Policy = "RequireMember")]
  // PUT	/api/comments/{id}	Update a comment (owner or Admin)	Authorized
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

  [HttpDelete("{id:guid}")]
  [Authorize(Policy = "RequireMember")]
  // DELETE	/api/comments/{id}	Soft delete a comment (owner or Admin)	Authorized
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
