using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize(Roles.Participant)]
public class ParticipantController : Controller
{
    private readonly ApiClient _api;

    public ParticipantController(ApiClient api)
    {
        _api = api;
    }

    // ---------- My enrolments ----------

    public async Task<IActionResult> MyEnrolments()
    {
        var result = await _api.GetAsync<List<EnrolmentDto>>("api/enrolments/mine");

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new List<EnrolmentDto>());
        }

        return View(result.Data ?? new List<EnrolmentDto>());
    }



    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var result = await _api.DeleteAsync($"api/enrolments/{id}");

        if (result.Success)
        {
            TempData["Success"] = "Your enrolment has been cancelled.";
        }
        else
        {
            // For example: a result has already been captured
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(MyEnrolments));
    }
    // ---------- My results ----------

    public async Task<IActionResult> MyResults()
    {
        var result = await _api.GetAsync<List<ResultDto>>("api/results/mine");

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return View(new List<ResultDto>());
        }

        return View(result.Data ?? new List<ResultDto>());
    }

    // ---------- Profile ----------

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var result = await _api.GetAsync<ParticipantProfileDto>("api/participants/me");

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.ErrorMessage ?? "We could not load your profile.";
            return RedirectToAction("Index", "Events");
        }

        return View(ToViewModel(result.Data));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _api.PutAsync<ParticipantProfileDto>("api/participants/me",
            new { vm.FullName, vm.PhoneNumber, vm.DateOfBirth });

        if (!result.Success || result.Data == null)
        {
            // For example: "Date of birth cannot be in the future."
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "We could not save your changes.");
            return View(vm);
        }

        // Keep the name shown in the menu up to date
        HttpContext.Session.SetString(SessionKeys.FullName, result.Data.FullName);

        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Profile));
    }

    // Copies the API's profile data into the form's view model
    private static ProfileViewModel ToViewModel(ParticipantProfileDto dto)
    {
        return new ProfileViewModel
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            ProfilePictureUrl = dto.ProfilePictureUrl
        };
    }

    // ---------- Profile picture ----------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile? picture)
    {
        if (picture == null || picture.Length == 0)
        {
            TempData["Error"] = "Please choose an image first.";
            return RedirectToAction(nameof(Profile));
        }

        var result = await _api.PostFileAsync<ParticipantProfileDto>("api/participants/me/picture", picture);

        if (result.Success)
        {
            TempData["Success"] = "Your profile picture has been updated.";
        }
        else
        {
            // For example: "Only image files (JPG, PNG, GIF or WEBP) are allowed."
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Profile));
    }


}