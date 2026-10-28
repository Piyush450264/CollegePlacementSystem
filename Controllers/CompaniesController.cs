
using CollegePlacementSystem.Data;
using CollegePlacementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CollegePlacementSystem.Controllers;
[Authorize(Roles = "Admin")]
public class CompaniesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CompaniesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var companies = await _context.Companies
            .AsNoTracking()
            .OrderBy(c => c.CompanyName)
            .ToListAsync();

        return View(companies);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyId == id);

        if (company == null)
            return NotFound();

        return View(company);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Company company)
    {
        if (ModelState.IsValid)
        {
            company.CreatedAt = DateTime.UtcNow;

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Company registered successfully.";
            return RedirectToAction(nameof(Index));
        }

        return View(company);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return NotFound();

        return View(company);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Company company)
    {
        if (id != company.CompanyId)
            return NotFound();

        if (!ModelState.IsValid)
            return View(company);

        var existingCompany = await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyId == id);

        if (existingCompany == null)
            return NotFound();

        existingCompany.CompanyName = company.CompanyName;
        existingCompany.Industry = company.Industry;
        existingCompany.ContactEmail = company.ContactEmail;
        existingCompany.ContactPhone = company.ContactPhone;
        existingCompany.Website = company.Website;
        existingCompany.Address = company.Address;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Company updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyId == id);

        if (company == null)
            return NotFound();

        return View(company);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company == null)
            return NotFound();

        var hasJobs = await _context.Jobs
            .AnyAsync(j => j.CompanyId == id);

        if (hasJobs)
        {
            TempData["Error"] =
                "This company has associated jobs and cannot be deleted.";

            return RedirectToAction(nameof(Index));
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Company deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}