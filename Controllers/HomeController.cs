using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
public class HomeController : ControllerBase
{
  [HttpGet("/index")]
  [AllowAnonymous]
  // [AllowAnonymous]
  public IActionResult Index()
  {
    return Content("Hello, World!");
  }
}
