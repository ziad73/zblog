using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
  public class HomeController : Controller
  {
    [HttpGet("/")]
    public IActionResult Index()
    {
      return Content("Hello, World!");
    }
  }
  
}
