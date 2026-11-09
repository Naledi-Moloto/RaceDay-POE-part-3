using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Venue
{
    [Key]
    public int VenueID { get; set; }

    [Required, MaxLength(150)]
    public string VenueName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AddressLine { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Province { get; set; } = string.Empty;

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }

    // One Venue -> many Events
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
