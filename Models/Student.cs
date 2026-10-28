
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CollegePlacementSystem.Models;

public class Student
{
    [Key]
    public int StudentId { get; set; }

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string EnrollmentNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Range(0, 10)]
    [Column(TypeName = "decimal(4,2)")]
    public decimal CGPA { get; set; }

    [Range(0, 100)]
    public int Backlogs { get; set; }

    [Phone, StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? ResumePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
