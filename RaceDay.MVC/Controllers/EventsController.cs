
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models.Events;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class EventsController : Controller
{
    private readonly IRaceDayApiService _apiService;

    public EventsController(IRaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("UserRole");

        if (role != "Organiser")
            return RedirectToAction("Login", "Auth");

        try
        {
            var events = await _apiService.GetAsync<List<EventViewModel>>(
                "/api/Events/mine");

            return View(events ?? new List<EventViewModel>());
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to load your events.";
            return View(new List<EventViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (HttpContext.Session.GetString("UserRole") != "Organiser")
            return RedirectToAction("Login", "Auth");

        await LoadLocations();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventViewModel model)
    {
        if (HttpContext.Session.GetString("UserRole") != "Organiser")
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            await LoadLocations();
            return View(model);
        }

        try
        {
            var response = await _apiService.PostAsync<EventViewModel, object>(
                "/api/Events", model);

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Could not create the event. Check your details.");
            await LoadLocations();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Organiser")
            return RedirectToAction("Login", "Auth");

        var model = await _apiService.GetAsync<EventViewModel>(
            $"/api/Events/{id}");

        if (model == null)
            return NotFound();

        await LoadLocations();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventViewModel model)
    {
        if (HttpContext.Session.GetString("UserRole") != "Organiser")
            return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            await LoadLocations();
            return View(model);
        }

        try
        {
            await _apiService.PutAsync<EventViewModel, object>(
                $"/api/Events/{id}", model);

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Could not update the event.");
            await LoadLocations();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Organiser")
            return RedirectToAction("Login", "Auth");

        var success = await _apiService.DeleteAsync($"/api/Events/{id}");

        TempData[success ? "Success" : "Error"] =
            success ? "Event deleted successfully." : "Could not delete the event.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLocations()
    {
        var locations = await _apiService.GetAsync<List<LocationViewModel>>(
            "/api/Locations");

        ViewBag.Locations = locations ?? new List<LocationViewModel>();
    }
}
