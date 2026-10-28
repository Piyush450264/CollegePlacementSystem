
using System.ComponentModel.DataAnnotations;

namespace CollegePlacementSystem.Models;

public class JobApplication
{
    [Key]
    public int ApplicationId { get; set; }

    [Required]
    public int StudentId { get; set; }

    public Student? Student { get; set; }

    [Required]
    public int JobId { get; set; }

    public Job? Job { get; set; }

    [Required, StringLength(30)]
    public string Status { get; set; } = "Applied";

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}
