using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Participant
{
    [Key]
    public int ParticipantID { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // One Participant -> many Enrolments
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}