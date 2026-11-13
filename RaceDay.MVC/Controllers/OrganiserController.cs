
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;

namespace RaceDay.MVC.Controllers;

[SessionAuthorize("Organiser")]
public class OrganiserController : Controller
{
    public IActionResult Index()
    {
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        return View();
    }
}
