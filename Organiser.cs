using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Organiser
{
    [Key]
    public int OrganiserID { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties (one Organiser -> many Events / Results)
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Result> CapturedResults { get; set; } = new List<Result>();
}
