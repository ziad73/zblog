using Microsoft.AspNetCore.Mvc;
using Models.Auth;
using Services.Auth.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Controllers;

[ApiController]
[Route("api/auth/")]
public class AuthController : Controller
{
  private readonly IAuthServices _authServices;
  private readonly ILogger<AuthController> _logger;

  public AuthController(IAuthServices authServices, ILogger<AuthController> logger)
  {
    _authServices = authServices;
    _logger = logger;
  }

  [HttpPost("register")]
  [AllowAnonymous]
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
  
  [HttpPost("login")]
  [AllowAnonymous]
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

  [HttpPost("logout")]
  [Authorize]
  public async Task<IActionResult> Logout()
  {
    var result = await _authServices.LogoutAsync();

    if (!result.IdentityResult.Succeeded)
    {
      return BadRequest(new ApiErrorResponseDto(
        "Logout failed.",
        result.IdentityResult.Errors.Select(e => e.Description)
      ));
    }

    return Ok(result.Response);
  }
}
