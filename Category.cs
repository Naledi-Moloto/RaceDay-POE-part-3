using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Category
{
    [Key]
    public int CategoryID { get; set; }

    // Foreign key
    public int EventID { get; set; }

    [Required, MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal DistanceKM { get; set; }

    public int? MinAge { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal EntryFee { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
