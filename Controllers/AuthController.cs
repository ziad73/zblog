using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.Auth;
using Models.Common;
using Services.Auth.Contracts;

namespace Controllers;

[ApiController]
[Route("api/auth/")]
public class AuthController : ControllerBase
{
  private readonly IAuthServices _authServices;
  private readonly ILogger<AuthController> _logger;

  public AuthController(IAuthServices authServices, ILogger<AuthController> logger)
  {
    _authServices = authServices;
    _logger = logger;
  }

  /// <summary>Registers a new user account and returns a JWT access token with a refresh token.</summary>
  /// <param name="registerRequestDto">Registration payload containing Name, Username, Email, Password, and ConfirmPassword.</param>
  /// <response code="201">User created. Returns user details, access token, and refresh token.</response>
  /// <response code="400">Validation failed or username/email already taken.</response>
  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody]RegisterRequestDto registerRequestDto)
  {
    // Built-in validation attributes auto-return 400 Bad Request before this if data is invalid.
    var result = await _authServices.RegisterAsync(registerRequestDto);

    // identity result check
    if (!result.IdentityResult.Succeeded)
    {
      return BadRequest(new ApiErrorResponseDto(
        "Registration failed.",
        result.IdentityResult.Errors.Select(e => e.Description)
      ));
    }

    return Created(string.Empty, result.Response);// 201 Created
  }
  
  /// <summary>Authenticates a user with username and password, returning a JWT access token and a refresh token.</summary>
  /// <param name="loginRequestDto">Login payload containing Username and Password.</param>
  /// <response code="200">Login successful. Returns user details, access token, and refresh token.</response>
  /// <response code="400">Invalid username or password.</response>
  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody]LoginRequestDto loginRequestDto)
  {
    // Built-in validation attributes auto-return 400 Bad Request before this if data is invalid.
    var result = await _authServices.LoginAsync(loginRequestDto);

    if (!result.IdentityResult.Succeeded)
    {
      return BadRequest(new ApiErrorResponseDto(
        "Login failed.",
        result.IdentityResult.Errors.Select(e => e.Description)
      ));
    }
    return Ok(result.Response);// 200 OK
  }

  /// <summary>Revokes the given refresh token, effectively logging the user out of that session.</summary>
  /// <param name="request">Payload containing the RefreshToken to revoke.</param>
  /// <response code="200">Logout successful. Refresh token revoked.</response>
  /// <response code="400">Refresh token not found or already inactive.</response>
  /// <response code="401">Unauthenticated request.</response>
  [HttpPost("logout")]
  [Authorize(Policy="RequireMember")]
  public async Task<IActionResult> Logout([FromBody] RevokeRequest request)
  {
    var result = await _authServices.LogoutAsync(request);

    if (!result.IdentityResult.Succeeded)
    {
      return BadRequest(new ApiErrorResponseDto(
        "Logout failed.",
        result.IdentityResult.Errors.Select(e => e.Description)
      ));
    }

    return Ok(result.Response);
  }

  /// <summary>Exchanges a valid refresh token for a new JWT access token and rotates the refresh token (single-use).</summary>
  /// <param name="request">Payload containing the current RefreshToken.</param>
  /// <response code="200">Token refreshed. Returns new access token and rotated refresh token.</response>
  /// <response code="401">Refresh token invalid, expired, or detected as reused (revokes all tokens for that user).</response>
  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
  {
      var result = await _authServices.RefreshAsync(request);

      if (!result.IdentityResult.Succeeded)
      {
          return Unauthorized(new ApiErrorResponseDto(
              "Token refresh failed.",
              result.IdentityResult.Errors.Select(e => e.Description)
          ));
      }

      return Ok(result.Response);
  }
}
