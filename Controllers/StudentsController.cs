
using CollegePlacementSystem.Data;
using CollegePlacementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;



namespace CollegePlacementSystem.Controllers;
[Authorize(Roles = "Admin")]
public class StudentsController : Controller
{
    [HttpGet]
public async Task<IActionResult> DownloadResume(int id)
{
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.StudentId == id);

    if (student == null)
    {
        return NotFound();
    }

    if (string.IsNullOrWhiteSpace(student.ResumePath))
    {
        return NotFound("This student has not uploaded a resume.");
    }

    var resumeFolder = Path.Combine(
        _environment.ContentRootPath,
        "PrivateResumes");

    var fileName = Path.GetFileName(student.ResumePath);
    var filePath = Path.Combine(resumeFolder, fileName);

    if (!System.IO.File.Exists(filePath))
    {
        return NotFound("The resume file could not be found.");
    }

    return PhysicalFile(
        filePath,
        "application/pdf",
        $"{student.EnrollmentNumber}_Resume.pdf");
}
    private readonly ApplicationDbContext _context;

    private readonly IWebHostEnvironment _environment;

private const long MaxResumeSize = 5 * 1024 * 1024;

    public StudentsController(
    ApplicationDbContext context,
    IWebHostEnvironment environment)
{
    _context = context;
    _environment = environment;
}

    // GET: /Students
    public async Task<IActionResult> Index()
    {
        var students = await _context.Students
            .AsNoTracking()
            .OrderBy(s => s.FullName)
            .ToListAsync();

        return View(students);
    }

    private async Task<string?> SaveResumeAsync(IFormFile? resumeFile)
{
    if (resumeFile == null || resumeFile.Length == 0)
        return null;

    if (resumeFile.Length > MaxResumeSize)
        throw new InvalidOperationException(
            "The resume file must not exceed 5 MB.");

    var extension = Path.GetExtension(resumeFile.FileName);

    if (!string.Equals(
            extension,
            ".pdf",
            StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Only PDF resume files are allowed.");
    }

    // Check the PDF file signature rather than relying only on its name.
    await using var input = resumeFile.OpenReadStream();

    var header = new byte[5];
    var bytesRead = await input.ReadAsync(header.AsMemory(0, 5));

    if (bytesRead != 5 ||
        header[0] != 0x25 ||
        header[1] != 0x50 ||
        header[2] != 0x44 ||
        header[3] != 0x46 ||
        header[4] != 0x2D)
    {
        throw new InvalidOperationException(
            "The uploaded file does not appear to be a valid PDF.");
    }

    var folder = Path.Combine(
        _environment.ContentRootPath,
        "PrivateResumes");

    Directory.CreateDirectory(folder);

    // Generate a random name; never use the supplied filename for storage.
    var storedFileName = $"{Guid.NewGuid():N}.pdf";
    var filePath = Path.Combine(folder, storedFileName);

    await using (var output = new FileStream(
        filePath,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.None))
    {
        await input.CopyToAsync(output);
    }

    return storedFileName;
}

    // GET: /Students/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == id);

        if (student == null)
            return NotFound();

        return View(student);
    }

    // GET: /Students/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Students/Create
   [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    Student student,
    IFormFile? resumeFile)
{
    if (await _context.Students.AnyAsync(
        s => s.Email == student.Email))
    {
        ModelState.AddModelError(
            nameof(student.Email),
            "This email address is already registered.");
    }

    if (await _context.Students.AnyAsync(
        s => s.EnrollmentNumber == student.EnrollmentNumber))
    {
        ModelState.AddModelError(
            nameof(student.EnrollmentNumber),
            "This enrollment number is already registered.");
    }

    if (!ModelState.IsValid)
        return View(student);

    try
    {
        student.ResumePath = await SaveResumeAsync(resumeFile);
    }
    catch (InvalidOperationException ex)
    {
        ModelState.AddModelError(nameof(resumeFile), ex.Message);
        return View(student);
    }

    student.CreatedAt = DateTime.UtcNow;

    _context.Students.Add(student);
    await _context.SaveChangesAsync();

    TempData["Success"] = "Student registered successfully.";

    return RedirectToAction(nameof(Index));
}

    // GET: /Students/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var student = await _context.Students.FindAsync(id);

        if (student == null)
            return NotFound();

        return View(student);
    }

    // POST: /Students/Edit/5
  [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(
    int id,
    Student student,
    IFormFile? resumeFile)
{
    if (id != student.StudentId)
        return NotFound();

    var existingStudent = await _context.Students
        .FirstOrDefaultAsync(s => s.StudentId == id);

    if (existingStudent == null)
        return NotFound();

    if (await _context.Students.AnyAsync(
        s => s.Email == student.Email &&
             s.StudentId != id))
    {
        ModelState.AddModelError(
            nameof(student.Email),
            "This email address is already registered.");
    }

    if (await _context.Students.AnyAsync(
        s => s.EnrollmentNumber == student.EnrollmentNumber &&
             s.StudentId != id))
    {
        ModelState.AddModelError(
            nameof(student.EnrollmentNumber),
            "This enrollment number is already registered.");
    }

    if (!ModelState.IsValid)
        return View(student);

    string? newResumePath = null;

    try
    {
        if (resumeFile != null && resumeFile.Length > 0)
        {
            newResumePath = await SaveResumeAsync(resumeFile);
        }
    }
    catch (InvalidOperationException ex)
    {
        ModelState.AddModelError(nameof(resumeFile), ex.Message);
        return View(student);
    }

    existingStudent.FullName = student.FullName;
    existingStudent.Email = student.Email;
    existingStudent.EnrollmentNumber = student.EnrollmentNumber;
    existingStudent.Department = student.Department;
    existingStudent.CGPA = student.CGPA;
    existingStudent.Backlogs = student.Backlogs;
    existingStudent.PhoneNumber = student.PhoneNumber;

    // Replace the stored resume path only if a new resume was uploaded.
    if (newResumePath != null)
    {
        var oldResumePath = existingStudent.ResumePath;
        existingStudent.ResumePath = newResumePath;

        // Delete the old file after the database update succeeds.
        try
        {
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(oldResumePath))
            {
                var oldFilePath = Path.Combine(
                    _environment.ContentRootPath,
                    "PrivateResumes",
                    Path.GetFileName(oldResumePath));

                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }
        }
        catch
        {
            var newFilePath = Path.Combine(
                _environment.ContentRootPath,
                "PrivateResumes",
                newResumePath);

            if (System.IO.File.Exists(newFilePath))
                System.IO.File.Delete(newFilePath);

            throw;
        }
    }
    else
    {
        await _context.SaveChangesAsync();
    }

    TempData["Success"] = "Student updated successfully.";

    return RedirectToAction(nameof(Index));
}

    // GET: /Students/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == id);

        if (student == null)
            return NotFound();

        return View(student);
    }

    // POST: /Students/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
            return NotFound();

        var hasApplications = await _context.JobApplications
            .AnyAsync(a => a.StudentId == id);

        if (hasApplications)
        {
            TempData["Error"] =
                "This student has job applications and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Student deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}