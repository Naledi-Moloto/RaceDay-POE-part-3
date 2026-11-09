using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize(Roles.Participant)]   // only logged-in Participants can use these pages
public class EventsController : Controller
{
    private readonly ApiClient _api;

    public EventsController(ApiClient api)
    {
        _api = api;
    }

    // ---------- Upcoming events with filters ----------

    public async Task<IActionResult> Index(string? search, string? type, string? province, DateOnly? fromDate)
    {
        var vm = new EventsIndexViewModel
        {
            Search = search,
            Type = type,
            Province = province,
            FromDate = fromDate
        };

        var result = await _api.GetAsync<List<EventDto>>("api/events");

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.ErrorMessage ?? "We could not load the events.";
            return View(vm);
        }

        // Only events that haven't happened yet
        var today = DateOnly.FromDateTime(DateTime.Today);
        var upcoming = result.Data.Where(e => e.EventDate >= today).ToList();

        vm.Provinces = upcoming.Select(e => e.Province).Distinct().OrderBy(p => p).ToList();

        // Apply the filters the user chose
        var filtered = upcoming.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered = filtered.Where(e =>
                e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.City.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.VenueName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            filtered = filtered.Where(e => e.EventType == type);
        }

        if (!string.IsNullOrWhiteSpace(province))
        {
            filtered = filtered.Where(e => e.Province == province);
        }

        if (fromDate.HasValue)
        {
            filtered = filtered.Where(e => e.EventDate >= fromDate.Value);
        }

        vm.Events = filtered.OrderBy(e => e.EventDate).ThenBy(e => e.StartTime).ToList();
        return View(vm);
    }

    // ---------- Full event information and categories ----------

    public async Task<IActionResult> Details(int id)
    {
        var eventResult = await _api.GetAsync<EventDto>($"api/events/{id}");

        if (!eventResult.Success || eventResult.Data == null)
        {
            TempData["Error"] = eventResult.ErrorMessage ?? "That event could not be found.";
            return RedirectToAction(nameof(Index));
        }

        var categoryResult = await _api.GetAsync<List<CategoryDto>>($"api/events/{id}/categories");
        var enrolmentResult = await _api.GetAsync<List<EnrolmentDto>>("api/enrolments/mine");

        if (!categoryResult.Success)
        {
            TempData["Error"] = categoryResult.ErrorMessage;
        }

        var vm = new EventDetailsViewModel
        {
            Event = eventResult.Data,
            Categories = categoryResult.Data ?? new List<CategoryDto>(),
            EnrolledCategoryIds = (enrolmentResult.Data ?? new List<EnrolmentDto>())
                .Where(e => e.EventID == id && e.Status == "Enrolled")
                .Select(e => e.CategoryID)
                .ToHashSet(),
            IsPast = eventResult.Data.EventDate < DateOnly.FromDateTime(DateTime.Today)
        };

        return View(vm);
    }

    // ---------- Enrol in a category ----------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var result = await _api.PostAsync<EnrolmentDto>("api/enrolments", new { categoryId });

        if (!result.Success || result.Data == null)
        {
            // For example: already enrolled, or below the minimum age. The API's message is shown.
            TempData["Error"] = result.ErrorMessage ?? "We could not complete your enrolment.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        TempData["Success"] = $"You're in! Your bib number is {result.Data.BibNumber}.";
        return RedirectToAction("MyEnrolments", "Participant");
    }
}