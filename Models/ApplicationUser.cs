using Microsoft.AspNetCore.Identity;

namespace CollegePlacementSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public int? StudentId { get; set; }
}