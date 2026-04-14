using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Web.Controllers;

/// <summary>Razor MVC landing page for the host root.</summary>
public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
