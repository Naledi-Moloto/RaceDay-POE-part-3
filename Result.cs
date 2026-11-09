using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Result
{
    [Key]
    public int ResultID { get; set; }

    // Foreign keys
    public int EnrolmentID { get; set; }
    public int CapturedByOrganiserID { get; set; }

    public TimeOnly? FinishTime { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DistanceCoveredKM { get; set; }

    public int? Position { get; set; }

    // Finished, DNF or DNS
    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    public DateTime CapturedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public Enrolment Enrolment { get; set; } = null!;
    public Organiser CapturedByOrganiser { get; set; } = null!;
}