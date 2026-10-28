
using CollegePlacementSystem.Data;
using CollegePlacementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegePlacementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalApplications =
            await _context.JobApplications.CountAsync();

        var selectedApplications =
            await _context.JobApplications.CountAsync(
                a => a.Status == "Selected");

        var model = new AdminDashboardViewModel
        {
            TotalStudents = await _context.Students.CountAsync(),

            TotalCompanies = await _context.Companies.CountAsync(),

            ActiveJobs = await _context.Jobs.CountAsync(
                j => j.ApplicationDeadline.Date >= DateTime.Today),

            TotalApplications = totalApplications,

            ShortlistedApplications =
                await _context.JobApplications.CountAsync(
                    a => a.Status == "Shortlisted"),

            SelectedApplications = selectedApplications,

            RejectedApplications =
                await _context.JobApplications.CountAsync(
                    a => a.Status == "Rejected"),

            PendingApplications =
                await _context.JobApplications.CountAsync(
                    a => a.Status == "Applied"),

            SelectionRate = totalApplications == 0
                ? 0
                : (double)selectedApplications / totalApplications * 100,

            RecentApplications = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Student)
                .Include(a => a.Job)
                    .ThenInclude(j => j!.Company)
                .OrderByDescending(a => a.AppliedAt)
                .Take(5)
                .Select(a => new RecentApplicationViewModel
                {
                    ApplicationId = a.ApplicationId,
                    StudentName = a.Student != null
                        ? a.Student.FullName
                        : "N/A",
                    JobTitle = a.Job != null
                        ? a.Job.JobTitle
                        : "N/A",
                    CompanyName = a.Job != null &&
                                  a.Job.Company != null
                        ? a.Job.Company.CompanyName
                        : "N/A",
                    Status = a.Status,
                    AppliedAt = a.AppliedAt
                })
                .ToListAsync()
        };

        return View(model);
    }
}
