
using CollegePlacementSystem.Data;
using CollegePlacementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CollegePlacementSystem.Controllers;

[Authorize(Roles = "Admin")]
public class JobsController : Controller
{
    private readonly ApplicationDbContext _context;

    public JobsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Jobs
    public async Task<IActionResult> Index()
    {
        var jobs = await _context.Jobs
            .AsNoTracking()
            .Include(j => j.Company)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return View(jobs);
    }

    // GET: /Jobs/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var job = await _context.Jobs
            .AsNoTracking()
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.JobId == id);

        if (job == null)
            return NotFound();

        return View(job);
    }

    // GET: /Jobs/Create
    [HttpGet]
public async Task<IActionResult> Create()
{
    await LoadCompanies();
    return View();
}

    // POST: /Jobs/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Job job)
    {
        await ValidateJob(job);

        if (!ModelState.IsValid)
        {
            await LoadCompanies(job.CompanyId);
            return View(job);
        }

        job.CreatedAt = DateTime.UtcNow;

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Job created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Jobs/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var job = await _context.Jobs.FindAsync(id);

        if (job == null)
            return NotFound();

        await LoadCompanies(job.CompanyId);

        return View(job);
    }

    // POST: /Jobs/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Job job)
    {
        if (id != job.JobId)
            return NotFound();

        await ValidateJob(job);

        if (!ModelState.IsValid)
        {
            await LoadCompanies(job.CompanyId);
            return View(job);
        }

        var existingJob = await _context.Jobs
            .FirstOrDefaultAsync(j => j.JobId == id);

        if (existingJob == null)
            return NotFound();

        existingJob.CompanyId = job.CompanyId;
        existingJob.JobTitle = job.JobTitle;
        existingJob.Description = job.Description;
        existingJob.EligibleDepartment = job.EligibleDepartment;
        existingJob.MinimumCGPA = job.MinimumCGPA;
        existingJob.MaximumBacklogs = job.MaximumBacklogs;
        existingJob.SalaryPackage = job.SalaryPackage;
        existingJob.ApplicationDeadline = job.ApplicationDeadline;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Job updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Jobs/Delete/5
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var job = await _context.Jobs
            .AsNoTracking()
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.JobId == id);

        if (job == null)
            return NotFound();

        return View(job);
    }

    // POST: /Jobs/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var job = await _context.Jobs.FindAsync(id);

        if (job == null)
            return NotFound();

        var hasApplications = await _context.JobApplications
            .AnyAsync(a => a.JobId == id);

        if (hasApplications)
        {
            TempData["Error"] =
                "This job has applications and cannot be deleted.";

            return RedirectToAction(nameof(Index));
        }

        _context.Jobs.Remove(job);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Job deleted successfully.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCompanies(int? selectedCompanyId = null)
    {
        var companies = await _context.Companies
            .AsNoTracking()
            .OrderBy(c => c.CompanyName)
            .ToListAsync();

        ViewBag.Companies = new SelectList(
            companies,
            "CompanyId",
            "CompanyName",
            selectedCompanyId);
    }

    private async Task ValidateJob(Job job)
    {
        if (!await _context.Companies.AnyAsync(
            c => c.CompanyId == job.CompanyId))
        {
            ModelState.AddModelError(
                nameof(job.CompanyId),
                "Please select a valid company.");
        }

        if (job.SalaryPackage < 0)
        {
            ModelState.AddModelError(
                nameof(job.SalaryPackage),
                "Salary package cannot be negative.");
        }

        if (job.ApplicationDeadline.Date < DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(job.ApplicationDeadline),
                "The application deadline cannot be in the past.");
        }
    }
}
