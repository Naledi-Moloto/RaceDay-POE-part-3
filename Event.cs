using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Event
{
    [Key]
    public int EventID { get; set; }

    // Foreign keys
    public int OrganiserID { get; set; }
    public int VenueID { get; set; }

    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    // Run, Walk or Cycle (checked in the database and in the DTOs)
    [Required, MaxLength(20)]
    public string EventType { get; set; } = string.Empty;

    public DateOnly EventDate { get; set; }

    public TimeOnly StartTime { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DistanceKM { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? BannerUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public Organiser Organiser { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}