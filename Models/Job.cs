
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePlacementSystem.Models;

public class Job
{
    [Key]
    public int JobId { get; set; }

    [Required]
    public int CompanyId { get; set; }

    public Company? Company { get; set; }

    [Required, StringLength(150)]
    public string JobTitle { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required, StringLength(100)]
    public string EligibleDepartment { get; set; } = string.Empty;

    [Range(0, 10)]
    [Column(TypeName = "decimal(4,2)")]
    public decimal MinimumCGPA { get; set; }

    [Range(0, 100)]
    public int MaximumBacklogs { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal SalaryPackage { get; set; }

    public DateTime ApplicationDeadline { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
