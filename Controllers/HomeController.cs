using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
public class HomeController : ControllerBase
{
  private readonly ILogger<HomeController> _logger;

  public HomeController(ILogger<HomeController> logger)
  {
    _logger = logger;
  }
  [HttpGet("/index")]
  [Authorize(Policy = "RequireMember")]
  public IActionResult Index()
  {
    _logger.LogInformation("Entered Index");
    return Content("Hello, World!");
  }
}
