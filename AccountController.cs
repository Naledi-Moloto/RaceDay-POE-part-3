using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api)
    {
        _api = api;
    }

    // ---------- Register ----------

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.IsLoggedIn())
        {
            return RedirectToRoleHome();
        }

        return View(new RegisterViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (vm.Role != Roles.Organiser && vm.Role != Roles.Participant)
        {
            ModelState.AddModelError(nameof(vm.Role), "Please choose a role.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        // Each role has its own register endpoint in the API (see the Part 1 plan)
        ApiResult<AuthResponse> result;

        if (vm.Role == Roles.Organiser)
        {
            result = await _api.PostAsync<AuthResponse>("api/auth/organiser/register",
                new { vm.FullName, vm.Email, vm.Password, vm.PhoneNumber });
        }
        else
        {
            result = await _api.PostAsync<AuthResponse>("api/auth/participant/register",
                new { vm.FullName, vm.Email, vm.Password, vm.PhoneNumber, vm.DateOfBirth });
        }

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Registration failed. Please try again.");
            return View(vm);
        }

        // The API logs the new user in straight away, so we do the same
        HttpContext.Session.Clear();
        HttpContext.Session.SaveLogin(result.Data);

        TempData["Success"] = $"Welcome to RaceDay, {result.Data.FullName}!";
        return RedirectToRoleHome();
    }

    // ---------- Login ----------

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.IsLoggedIn())
        {
            return RedirectToRoleHome();
        }

        return View(new LoginViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (vm.Role != Roles.Organiser && vm.Role != Roles.Participant)
        {
            ModelState.AddModelError(nameof(vm.Role), "Please choose a role.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var endpoint = vm.Role == Roles.Organiser
            ? "api/auth/organiser/login"
            : "api/auth/participant/login";

        var result = await _api.PostAsync<AuthResponse>(endpoint, new { vm.Email, vm.Password });

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Login failed. Please try again.");
            return View(vm);
        }

        HttpContext.Session.Clear();
        HttpContext.Session.SaveLogin(result.Data);

        return RedirectToRoleHome();
    }

    // ---------- Logout ----------

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }

    // ---------- Access denied ----------

    public IActionResult AccessDenied()
    {
        return View();
    }

    // ---------- Helper ----------

    // Where a user lands after logging in or registering
    private IActionResult RedirectToRoleHome()
    {
        return HttpContext.Session.GetRole() == Roles.Organiser
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Events");
    }
}