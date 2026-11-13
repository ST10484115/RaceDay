using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Filters;
using RaceDay.MVC.Models.Events;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[SessionAuthorize("Organiser")]
public class CategoriesController : Controller
{
    private readonly IRaceDayApiService _apiService;

    public CategoriesController(IRaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index(int eventId)
    {
        try
        {
            var categories = await _apiService.GetAsync<List<CategoryViewModel>>(
                $"/api/Categories/event/{eventId}");

            ViewBag.EventId = eventId;

            return View(categories ?? new List<CategoryViewModel>());
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "Unable to load categories.";
            ViewBag.EventId = eventId;

            return View(new List<CategoryViewModel>());
        }
    }

    [HttpGet]
    public IActionResult Create(int eventId)
    {
        ViewBag.EventId = eventId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.EventId = model.EventId;
            return View(model);
        }

        try
        {
            await _apiService.PostAsync<CategoryViewModel, object>(
                $"/api/Categories/event/{model.EventId}", model);

            TempData["Success"] = "Category created successfully!";
            return RedirectToAction(nameof(Index), new { eventId = model.EventId });
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Could not create the category.");
            ViewBag.EventId = model.EventId;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _apiService.GetAsync<CategoryViewModel>(
            $"/api/Categories/{id}");

        if (category == null)
            return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _apiService.PutAsync<CategoryViewModel, object>(
                $"/api/Categories/{id}", model);

            TempData["Success"] = "Category updated successfully!";
            return RedirectToAction(nameof(Index), new { eventId = model.EventId });
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Could not update the category.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        var success = await _apiService.DeleteAsync($"/api/Categories/{id}");

        TempData[success ? "Success" : "Error"] =
            success ? "Category deleted successfully!" : "Could not delete the category.";

        return RedirectToAction(nameof(Index), new { eventId });
    }
}