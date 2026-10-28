
using CollegePlacementSystem.Data;
using CollegePlacementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegePlacementSystem.Controllers;

[Authorize]
public class JobApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobApplicationsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Admin: View all applications
    [Authorize(Roles = "Admin")]
public async Task<IActionResult> Index()
{
    var applications = await _context.JobApplications
        .AsNoTracking()
        .Include(a => a.Student)
        .Include(a => a.Job)
            .ThenInclude(j => j!.Company)
        .OrderByDescending(a => a.AppliedAt)
        .ToListAsync();

    return View(applications);
}

    // Admin and Student: View available jobs
[Authorize(Roles = "Admin,Student")]
public async Task<IActionResult> AvailableJobs()
{
    var jobs = await _context.Jobs
        .AsNoTracking()
        .Include(j => j.Company)
        .Where(j => j.ApplicationDeadline >= DateTime.Today)
        .OrderBy(j => j.ApplicationDeadline)
        .ToListAsync();

    // Admins can view jobs without student-specific eligibility.
    if (User.IsInRole("Admin"))
    {
        return View(jobs);
    }

    // Get the logged-in student's linked StudentId.
    var studentId = await GetCurrentStudentIdAsync();

    if (studentId == null)
    {
        return Forbid();
    }

    var student = await _context.Students
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.StudentId == studentId.Value);

    if (student == null)
    {
        return Forbid();
    }

    // Pass the student's details to the view.
    ViewBag.StudentDepartment = student.Department;
    ViewBag.StudentCGPA = student.CGPA;
    ViewBag.StudentBacklogs = student.Backlogs;

    // Pass eligibility for each job to the view.
    ViewBag.JobEligibility = jobs.ToDictionary(
        job => job.JobId,
        job => IsEligible(student, job));

    return View(jobs);
}

    // Student: Open the application form
    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> ApplyForm(int id)
    {
        var job = await _context.Jobs
            .AsNoTracking()
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.JobId == id);

        if (job == null)
            return NotFound();

        if (job.ApplicationDeadline.Date < DateTime.Today)
        {
            TempData["Error"] = "The application deadline has passed.";
            return RedirectToAction(nameof(AvailableJobs));
        }

        var studentId = await GetCurrentStudentIdAsync();

        if (studentId == null)
        {
            return Forbid();
        }

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == studentId.Value);

        if (student == null)
            return Forbid();

        if (!IsEligible(student, job))
        {
            TempData["Error"] =
                "You do not meet the eligibility requirements for this job.";

            return RedirectToAction(nameof(AvailableJobs));
        }

        bool alreadyApplied = await _context.JobApplications
            .AnyAsync(a =>
                a.JobId == id &&
                a.StudentId == studentId.Value);

        if (alreadyApplied)
        {
            TempData["Error"] = "You have already applied for this job.";
            return RedirectToAction(nameof(MyApplications));
        }

        ViewBag.Job = job;

        return View();
    }

    // Student: Submit an application for their own account
    [Authorize(Roles = "Student")]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Apply(int jobId)
{
    var studentId = await GetCurrentStudentIdAsync();

    if (studentId == null)
    {
        return Forbid();
    }

    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.StudentId == studentId.Value);

    if (student == null)
    {
        return Forbid();
    }

    var job = await _context.Jobs
        .Include(j => j.Company)
        .FirstOrDefaultAsync(j => j.JobId == jobId);

    if (job == null)
    {
        return NotFound();
    }

    // Check whether the application deadline has passed.
    if (job.ApplicationDeadline.Date < DateTime.Today)
    {
        TempData["Error"] = "The application deadline has passed.";
        return RedirectToAction(nameof(AvailableJobs));
    }

    // Check whether the student meets the job requirements.
    if (!IsEligible(student, job))
    {
        TempData["Error"] =
            "You are not eligible for this job based on its requirements.";

        return RedirectToAction(nameof(AvailableJobs));
    }

    // Prevent duplicate applications.
    bool alreadyApplied = await _context.JobApplications
        .AnyAsync(a =>
            a.StudentId == studentId.Value &&
            a.JobId == jobId);

    if (alreadyApplied)
    {
        TempData["Error"] = "You have already applied for this job.";
        return RedirectToAction(nameof(MyApplications));
    }

    var application = new JobApplication
    {
        StudentId = studentId.Value,
        JobId = jobId,
        Status = "Applied",
        AppliedAt = DateTime.Now
    };

    _context.JobApplications.Add(application);

    await _context.SaveChangesAsync();

    TempData["Success"] = "Your application was submitted successfully!";

    return RedirectToAction(nameof(MyApplications));
}

    // Student: View only their own applications
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyApplications()
    {
        var studentId = await GetCurrentStudentIdAsync();

        if (studentId == null)
            return Forbid();

        var applications = await _context.JobApplications
            .AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Job)
                .ThenInclude(j => j!.Company)
            .Where(a => a.StudentId == studentId.Value)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return View(applications);
    }

    // Admin: Update an application's status
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var allowedStatuses = new[]
        {
            "Applied",
            "Shortlisted",
            "Selected",
            "Rejected"
        };

        if (string.IsNullOrWhiteSpace(status) ||
            !allowedStatuses.Contains(status))
        {
            return BadRequest("Invalid application status.");
        }

        var application = await _context.JobApplications
            .FirstOrDefaultAsync(a => a.ApplicationId == id);

        if (application == null)
            return NotFound();

        application.Status = status;

        await _context.SaveChangesAsync();

        TempData["Success"] =
            "Application status updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<int?> GetCurrentStudentIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);

        return user?.StudentId;
    }

    private bool IsEligible(Student student, Job job)
{
    // Check whether the department matches.
    var eligibleDepartment = job.EligibleDepartment?.Trim();

    bool departmentEligible =
        string.IsNullOrWhiteSpace(eligibleDepartment) ||
        eligibleDepartment.Equals(
            "ALL",
            StringComparison.OrdinalIgnoreCase) ||
        eligibleDepartment.Equals(
            "All Departments",
            StringComparison.OrdinalIgnoreCase) ||
        eligibleDepartment.Equals(
            student.Department?.Trim(),
            StringComparison.OrdinalIgnoreCase);

    // Check minimum CGPA.
    bool cgpaEligible =
        student.CGPA >= job.MinimumCGPA;

    // Check maximum permitted backlogs.
    bool backlogsEligible =
        student.Backlogs <= job.MaximumBacklogs;

    return departmentEligible &&
           cgpaEligible &&
           backlogsEligible;
}
}
