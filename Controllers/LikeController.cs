using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Common;
using Models.Like;
using Services.Like.Contracts;

namespace Controllers;

[ApiController]
[Route("api/likes")]
public class LikeController : ControllerBase
{
  private readonly ILikeServices _likeServices;

  public LikeController(ILikeServices likeServices)
  {
    _likeServices = likeServices;
  }

  /// <summary>Like a blog post or a comment.</summary>
  /// <param name="dto">Payload with exactly one of PostId or CommentId.</param>
  /// <response code="201">Like created.</response>
  /// <response code="400">Invalid target or duplicate like.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not a member/author/admin.</response>
  [HttpPost]
  [Authorize(Policy = "RequireMember")]
  public async Task<IActionResult> Like([FromBody] LikeRequestDto dto)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var result = await _likeServices.LikeAsync(userId, dto);

    if (result is null)
      return BadRequest(new ApiErrorResponseDto(
        "Like failed.",
        new List<string> { "Invalid target, duplicate like, or target not found." }
      ));

    return Created(string.Empty, result);
  }

  /// <summary>Unlike a blog post or a comment.</summary>
  /// <param name="postId">The post GUID to unlike (omit if unliking a comment).</param>
  /// <param name="commentId">The comment GUID to unlike (omit if unliking a post).</param>
  /// <response code="204">Like removed successfully.</response>
  /// <response code="400">Invalid target or like not found.</response>
  /// <response code="401">Unauthenticated request.</response>
  /// <response code="403">Authenticated but not a member/author/admin.</response>
  [HttpDelete]
  [Authorize(Policy = "RequireMember")]
  public async Task<IActionResult> Unlike([FromQuery] Guid? postId, [FromQuery] Guid? commentId)
  {
    var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    var deleted = await _likeServices.UnlikeAsync(userId, postId, commentId);

    if (!deleted)
      return BadRequest(new ApiErrorResponseDto(
        "Unlike failed.",
        new List<string> { "Invalid target or like not found." }
      ));

    return NoContent();
  }
}
