
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models.Authentication;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class AuthController : Controller
{
    private readonly IRaceDayApiService _apiService;

    public AuthController(IRaceDayApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _apiService.PostAsync<
    RegisterViewModel,
    AuthResponseViewModel>(
        "/api/Auth/register",
        new RegisterViewModel
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Password = model.Password,
            Role = model.Role
        });

            if (response == null)
            {
                ModelState.AddModelError("", "Registration failed.");
                return View(model);
            }

            TempData["Success"] = "Registration successful. Please log in.";
            return RedirectToAction(nameof(Login));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "", "Unable to register. Check your details and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _apiService.PostAsync<
                LoginViewModel, AuthResponseViewModel>(
                "/api/Auth/login", model);

            if (response == null)
            {
                ModelState.AddModelError("", "Login failed.");
                return View(model);
            }

            HttpContext.Session.SetString("UserRole", response.Role);
            HttpContext.Session.SetInt32("UserId", response.UserId);
            HttpContext.Session.SetString("UserName", response.FirstName);

            if (response.Role == "Organiser")
                return RedirectToAction("Index", "Organiser");

            return RedirectToAction("Index", "Participant");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "", "Invalid login details or API unavailable.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _apiService.PostAsync<object, AuthResponseViewModel>(
                "/api/Auth/logout", new { });
        }
        catch (HttpRequestException)
        {
            // Continue clearing the MVC session even if the API is unavailable.
        }

        HttpContext.Session.Clear();

        return RedirectToAction(nameof(Login));
    }
}
