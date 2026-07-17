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

  /// <summary>
  /// Register a new user
  /// </summary>
  /// <param name="registerRequestDto"> RegisterRequestDto </param>
  /// <returns> IActionResult </returns>
  [HttpPost("register")]
  // [AllowAnonymous] public to use 
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
  
  /// <summary>
  /// Login a user
  /// </summary>
  /// <param name="loginRequestDto"> LoginRequestDto </param>
  /// <returns> IActionResult </returns>
  [HttpPost("login")]
  // [AllowAnonymous] public to use 
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

  /// <summary>
  /// Logout a user
  /// </summary>
  /// <returns> IActionResult </returns>
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

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
  {
    /* called by the client (frontend/mobile app) when the access token (JWT) expires. 
      - The client sends the refresh token to get a new access token without requiring the user to log in again.
      - This happens transparently in the background.*/
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
