using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public class DashboardViewModel
{
    public List<EventDto> UpcomingEvents { get; set; } = new();
    public List<EventDto> PastEvents { get; set; } = new();
    public int TotalEvents { get; set; }
    public int TotalEnrolments { get; set; }
}

// Used for both creating and editing an event
public class EventFormViewModel
{
    public int EventId { get; set; }   // 0 means "creating a new event"

    [Required(ErrorMessage = "Please enter the event name."), StringLength(150)]
    [Display(Name = "Event name")]
    public string EventName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please add a short description.")]
    [StringLength(1000, ErrorMessage = "The description is too long (maximum 1000 characters).")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Please choose the event date.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date")]
    public DateOnly? EventDate { get; set; }

    [Required(ErrorMessage = "Please choose the start time.")]
    [DataType(DataType.Time)]
    [Display(Name = "Start time")]
    public TimeOnly? StartTime { get; set; }

    [Required(ErrorMessage = "Please select a venue.")]
    [Display(Name = "Venue (location)")]
    public int? VenueId { get; set; }

    [Required(ErrorMessage = "Please enter the distance.")]
    [Range(0.01, 999.99, ErrorMessage = "Distance must be between 0.01 and 999.99 km.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKM { get; set; }

    [Required(ErrorMessage = "Please choose the event type.")]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = "Run";

    // Fills the venue dropdown. It is not posted back, so we reload it when needed.

    // Optional banner image chosen by the Organiser
    [Display(Name = "Banner image")]
    public IFormFile? Banner { get; set; }

    // The banner the event already has (shown when editing)
    public string? BannerUrl { get; set; }


    public List<VenueDto> Venues { get; set; } = new();
}

public class VenueFormViewModel
{
    public static readonly string[] Provinces =
    {
        "Eastern Cape", "Free State", "Gauteng", "KwaZulu-Natal", "Limpopo",
        "Mpumalanga", "North West", "Northern Cape", "Western Cape"
    };

    [Required(ErrorMessage = "Please enter the venue name."), StringLength(150)]
    [Display(Name = "Venue name")]
    public string VenueName { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Address")]
    public string? AddressLine { get; set; }

    [Required(ErrorMessage = "Please enter the city or town."), StringLength(100)]
    [Display(Name = "City / town")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose a province.")]
    public string Province { get; set; } = string.Empty;

    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
    public decimal? Latitude { get; set; }

    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
    public decimal? Longitude { get; set; }
}

public class CategoriesViewModel
{
    public EventDto Event { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public CategoryFormViewModel Form { get; set; } = new();
}

// Used for both adding and editing a category
public class CategoryFormViewModel
{
    public int CategoryId { get; set; }   // 0 means "adding a new category"
    public int EventId { get; set; }

    [Required(ErrorMessage = "Please enter a category name."), StringLength(100)]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the distance.")]
    [Range(0.01, 999.99, ErrorMessage = "Distance must be between 0.01 and 999.99 km.")]
    [Display(Name = "Distance (km)")]
    public decimal? DistanceKM { get; set; }

    [Range(0, 120, ErrorMessage = "Minimum age must be between 0 and 120.")]
    [Display(Name = "Minimum age (optional)")]
    public int? MinAge { get; set; }

    [Required(ErrorMessage = "Please enter the entry fee (0 for free).")]
    [Range(0, 99999.99, ErrorMessage = "The entry fee must be between 0 and 99999.99.")]
    [Display(Name = "Entry fee (R)")]
    public decimal? EntryFee { get; set; } = 0;
}

public class EventEnrolmentsViewModel
{
    public EventDto Event { get; set; } = new();
    public List<EnrolmentDto> Enrolments { get; set; } = new();
}

public class ResultsViewModel
{
    public EventDto Event { get; set; } = new();
    public List<ResultRowViewModel> Rows { get; set; } = new();
    public bool EventHasEnded { get; set; }
}

// One participant: their enrolment plus their result (if one has been captured)
public class ResultRowViewModel
{
    public EnrolmentDto Enrolment { get; set; } = new();
    public ResultDto? Result { get; set; }
}

// What one row of the Results page posts back
public class ResultFormViewModel
{
    public int EventId { get; set; }
    public int EnrolmentId { get; set; }
    public int ResultId { get; set; }   // 0 means "no result captured yet"
    public string Status { get; set; } = "Finished";
    public TimeOnly? FinishTime { get; set; }
    public int? Position { get; set; }
    public decimal? DistanceCoveredKM { get; set; }
}
