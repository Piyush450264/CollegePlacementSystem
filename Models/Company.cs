
using System.ComponentModel.DataAnnotations;

namespace CollegePlacementSystem.Models;

public class Company
{
    [Key]
    public int CompanyId { get; set; }

    [Required, StringLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Industry { get; set; }

    [EmailAddress, StringLength(150)]
    public string? ContactEmail { get; set; }

    [Phone, StringLength(20)]
    public string? ContactPhone { get; set; }

    [StringLength(255)]
    public string? Website { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
