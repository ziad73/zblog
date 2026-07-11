using Microsoft.AspNetCore.Mvc;
using Models.Auth;
using Services.Contracts;
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

    return Created(string.Empty, result.Response);
  }

  // public IActionResult Regis
  // POST	/api/account/login	Login a user
  // POST	/api/account/logout	Logout the current user
}
