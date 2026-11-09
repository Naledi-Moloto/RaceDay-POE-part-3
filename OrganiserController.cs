using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[SessionAuthorize(Roles.Organiser)]   // only logged-in Organisers can use these pages
public class OrganiserController : Controller
{
    private readonly ApiClient _api;

    public OrganiserController(ApiClient api)
    {
        _api = api;
    }

    // ---------- Dashboard ----------

    public async Task<IActionResult> Dashboard()
    {
        var vm = new DashboardViewModel();
        var result = await GetMyEventsAsync();

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.ErrorMessage ?? "We could not load your events.";
            return View(vm);
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var mine = result.Data;

        vm.TotalEvents = mine.Count;
        vm.TotalEnrolments = mine.Sum(e => e.EnrolmentCount);
        vm.UpcomingEvents = mine.Where(e => e.EventDate >= today)
            .OrderBy(e => e.EventDate).ThenBy(e => e.StartTime).ToList();
        vm.PastEvents = mine.Where(e => e.EventDate < today)
            .OrderByDescending(e => e.EventDate).ToList();

        return View(vm);
    }

    // ---------- My events ----------

    public async Task<IActionResult> Events()
    {
        var result = await GetMyEventsAsync();

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.ErrorMessage ?? "We could not load your events.";
            return View(new List<EventDto>());
        }

        return View(result.Data.OrderByDescending(e => e.EventDate).ToList());
    }

    // ---------- Create and edit an event ----------

    [HttpGet]
    public async Task<IActionResult> CreateEvent()
    {
        var vm = new EventFormViewModel();
        await LoadVenuesAsync(vm);
        return View("EventForm", vm);
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var result = await _api.GetAsync<EventDto>($"api/events/{id}");

        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.ErrorMessage ?? "That event could not be found.";
            return RedirectToAction(nameof(Events));
        }

        var ev = result.Data;

        if (ev.OrganiserID != HttpContext.Session.GetUserId())
        {
            TempData["Error"] = "You can only edit events that you created.";
            return RedirectToAction(nameof(Events));
        }

        var vm = new EventFormViewModel
        {
            EventId = ev.EventID,
            EventName = ev.EventName,
            Description = ev.Description,
            EventDate = ev.EventDate,
            StartTime = ev.StartTime,
            VenueId = ev.VenueID,
            DistanceKM = ev.DistanceKM,
            EventType = ev.EventType,
            BannerUrl = ev.BannerUrl
        };

        await LoadVenuesAsync(vm);
        return View("EventForm", vm);
    }

    // One action saves both new and existing events
    // One action saves both new and existing events
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEvent(EventFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadVenuesAsync(vm);
            return View("EventForm", vm);
        }

        // The field names match the request body in the Part 1 plan
        var body = new
        {
            vm.EventName,
            vm.Description,
            vm.EventDate,
            vm.StartTime,
            vm.VenueId,
            vm.DistanceKM,
            vm.EventType
        };

        ApiResult<EventDto> result = vm.EventId == 0
            ? await _api.PostAsync<EventDto>("api/events", body)
            : await _api.PutAsync<EventDto>($"api/events/{vm.EventId}", body);

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "We could not save the event.");
            await LoadVenuesAsync(vm);
            return View("EventForm", vm);
        }

        // Upload the banner if one was chosen. The file goes to the API, never straight to Azure.
        // (An empty file box still arrives as a file with length 0, so we check the length.)
        if (vm.Banner != null && vm.Banner.Length > 0)
        {
            var upload = await _api.PostFileAsync<EventDto>($"api/events/{result.Data.EventID}/banner", vm.Banner);

            if (!upload.Success)
            {
                // The event itself is saved. Only the picture failed, so send them to the edit page to try again.
                TempData["Error"] = $"Your event was saved, but the banner could not be uploaded. {upload.ErrorMessage}";
                return RedirectToAction(nameof(EditEvent), new { id = result.Data.EventID });
            }
        }

        TempData["Success"] = vm.EventId == 0 ? "Your event has been created." : "Your event has been updated.";
        return RedirectToAction(nameof(Events));
    }

    // ---------- Delete an event ----------

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await _api.DeleteAsync($"api/events/{id}");

        if (result.Success)
        {
            TempData["Success"] = "The event has been deleted.";
        }
        else
        {
            // For example: "This event has enrolments and cannot be deleted."
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Events));
    }

    // ---------- Add a venue ----------

    [HttpGet]
    public IActionResult CreateVenue()
    {
        return View(new VenueFormViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVenue(VenueFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var result = await _api.PostAsync<VenueDto>("api/venues",
            new { vm.VenueName, vm.AddressLine, vm.City, vm.Province, vm.Latitude, vm.Longitude });

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "We could not save the venue.");
            return View(vm);
        }

        TempData["Success"] = "Venue added. You can now choose it below.";
    
        return RedirectToAction(nameof(CreateEvent));


    }


    // ---------- Categories of an event ----------

    [HttpGet]
    public Task<IActionResult> Categories(int id, int? editId)
    {
        return ShowCategoriesAsync(id, null, editId);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CategoryFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            // Show the same page again, with the form's messages
            return await ShowCategoriesAsync(form.EventId, form);
        }

        var body = new { form.CategoryName, form.DistanceKM, form.MinAge, form.EntryFee };

        ApiResult<CategoryDto> result = form.CategoryId == 0
            ? await _api.PostAsync<CategoryDto>($"api/events/{form.EventId}/categories", body)
            : await _api.PutAsync<CategoryDto>($"api/categories/{form.CategoryId}", body);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "We could not save the category.");
            return await ShowCategoriesAsync(form.EventId, form);
        }

        TempData["Success"] = form.CategoryId == 0 ? "Category added." : "Category updated.";
        return RedirectToAction(nameof(Categories), new { id = form.EventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var result = await _api.DeleteAsync($"api/categories/{id}");

        if (result.Success)
        {
            TempData["Success"] = "The category has been deleted.";
        }
        else
        {
            // For example: "This category has enrolments and cannot be deleted."
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Categories), new { id = eventId });
    }

    // ---------- Enrolments for an event ----------

    public async Task<IActionResult> Enrolments(int id)
    {
        var (ev, error) = await GetOwnEventAsync(id);

        if (ev == null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Events));
        }

        var result = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
        }

        return View(new EventEnrolmentsViewModel
        {
            Event = ev,
            Enrolments = result.Data ?? new List<EnrolmentDto>()
        });
    }

    // ---------- Results for an event ----------

    public async Task<IActionResult> Results(int id)
    {
        var (ev, error) = await GetOwnEventAsync(id);

        if (ev == null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Events));
        }

        var enrolmentResult = await _api.GetAsync<List<EnrolmentDto>>($"api/events/{id}/enrolments");

        if (!enrolmentResult.Success || enrolmentResult.Data == null)
        {
            TempData["Error"] = enrolmentResult.ErrorMessage ?? "We could not load the enrolments.";
            return RedirectToAction(nameof(Events));
        }

        var resultResult = await _api.GetAsync<List<ResultDto>>($"api/events/{id}/results");

        // Look up each enrolment's result by the enrolment ID
        var resultsByEnrolment = (resultResult.Data ?? new List<ResultDto>())
            .ToDictionary(r => r.EnrolmentID);

        // Cancelled enrolments cannot have a result, so they are left out
        var rows = enrolmentResult.Data
            .Where(e => e.Status == "Enrolled")
            .OrderBy(e => e.CategoryName)
            .ThenBy(e => e.ParticipantName)
            .Select(e => new ResultRowViewModel
            {
                Enrolment = e,
                Result = resultsByEnrolment.TryGetValue(e.EnrolmentID, out var existing) ? existing : null
            })
            .ToList();

        return View(new ResultsViewModel
        {
            Event = ev,
            Rows = rows,
            EventHasEnded = ev.EventDate < DateOnly.FromDateTime(DateTime.Today)
        });
    }

    // Saves one participant's result. Saving also publishes it: the participant sees it straight away.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResult(ResultFormViewModel form)
    {
        ApiResult<ResultDto> result = form.ResultId == 0
            ? await _api.PostAsync<ResultDto>("api/results",
                new { form.EnrolmentId, form.Status, form.FinishTime, form.Position, form.DistanceCoveredKM })
            : await _api.PutAsync<ResultDto>($"api/results/{form.ResultId}",
                new { form.Status, form.FinishTime, form.Position, form.DistanceCoveredKM });

        if (result.Success)
        {
            TempData["Success"] = "Result saved and published to the participant.";
        }
        else
        {
            // The API explains what is wrong, for example "A finished result needs a finish time."
            TempData["Error"] = result.ErrorMessage;
        }

        return RedirectToAction(nameof(Results), new { id = form.EventId });
    }


    // ---------- Helpers ----------

    // Loads an event and checks that it belongs to the logged-in Organiser
    private async Task<(EventDto? Event, string? Error)> GetOwnEventAsync(int id)
    {
        var result = await _api.GetAsync<EventDto>($"api/events/{id}");

        if (!result.Success || result.Data == null)
        {
            return (null, result.ErrorMessage ?? "That event could not be found.");
        }

        if (result.Data.OrganiserID != HttpContext.Session.GetUserId())
        {
            return (null, "You can only manage events that you created.");
        }

        return (result.Data, null);
    }

    // Builds and shows the Categories page. If no form is given, it makes an empty one
    // (or one pre-filled for editing when editId is set).
    private async Task<IActionResult> ShowCategoriesAsync(int eventId, CategoryFormViewModel? form, int? editId = null)
    {
        var (ev, error) = await GetOwnEventAsync(eventId);

        if (ev == null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Events));
        }

        var result = await _api.GetAsync<List<CategoryDto>>($"api/events/{eventId}/categories");

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
        }

        var categories = result.Data ?? new List<CategoryDto>();

        if (form == null)
        {
            form = new CategoryFormViewModel { EventId = eventId };

            var toEdit = editId.HasValue ? categories.FirstOrDefault(c => c.CategoryID == editId.Value) : null;

            if (toEdit != null)
            {
                form.CategoryId = toEdit.CategoryID;
                form.CategoryName = toEdit.CategoryName;
                form.DistanceKM = toEdit.DistanceKM;
                form.MinAge = toEdit.MinAge;
                form.EntryFee = toEdit.EntryFee;
            }
        }

        return View("Categories", new CategoriesViewModel
        {
            Event = ev,
            Categories = categories,
            Form = form
        });
    }

    // The API returns everyone's events, so we keep only the ones this Organiser created
    private async Task<ApiResult<List<EventDto>>> GetMyEventsAsync()
    {
        var result = await _api.GetAsync<List<EventDto>>("api/events");

        if (result.Success && result.Data != null)
        {
            var myId = HttpContext.Session.GetUserId();
            result.Data = result.Data.Where(e => e.OrganiserID == myId).ToList();
        }

        return result;
    }

    private async Task LoadVenuesAsync(EventFormViewModel vm)
    {
        var result = await _api.GetAsync<List<VenueDto>>("api/venues");
        vm.Venues = result.Data ?? new List<VenueDto>();
    }
}