using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Logged-in users go straight to their own home page
        if (HttpContext.Session.IsLoggedIn())
        {
            return HttpContext.Session.GetRole() == Roles.Organiser
                ? RedirectToAction("Dashboard", "Organiser")
                : RedirectToAction("Index", "Events");
        }

        return View();
    }



    // Shown for things like "page not found" (404)
    public IActionResult StatusPage(int code)
    {
        return View(code);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}