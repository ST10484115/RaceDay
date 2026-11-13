
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;

namespace RaceDay.MVC.Controllers;

[SessionAuthorize("Participant")]
public class ParticipantController : Controller
{
    public IActionResult Index()
    {
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        return View();
    }
}
