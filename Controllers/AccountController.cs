using CollegePlacementSystem.Data;
using CollegePlacementSystem.Models;
using CollegePlacementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegePlacementSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var email = model.Email.Trim();
        var enrollmentNumber = model.EnrollmentNumber.Trim();

        var student = await _context.Students.FirstOrDefaultAsync(s =>
            s.Email == email &&
            s.EnrollmentNumber == enrollmentNumber);

        if (student == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "No matching student record was found. Check your email and enrollment number.");

            return View(model);
        }

        var existingLinkedAccount = await _context.Users
            .AnyAsync(u => u.StudentId == student.StudentId);

        if (existingLinkedAccount)
        {
            ModelState.AddModelError(
                string.Empty,
                "An account is already registered for this student.");

            return View(model);
        }

        var existingEmail = await _userManager.FindByEmailAsync(email);

        if (existingEmail != null)
        {
            ModelState.AddModelError(
                string.Empty,
                "An account with this email already exists.");

            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = student.FullName,
            StudentId = student.StudentId,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // A Student role must exist before registration can succeed.
        var roleExists = await _context.Roles
            .AnyAsync(r => r.NormalizedName == "STUDENT");

        if (!roleExists)
        {
            await _userManager.DeleteAsync(user);

            ModelState.AddModelError(
                string.Empty,
                "Student registration is not configured yet. Please try again later.");

            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "Student");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            ModelState.AddModelError(
                string.Empty,
                "The student account could not be assigned its role.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Registration successful. You can now log in.";

        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Dashboard");

            return RedirectToAction("AvailableJobs", "JobApplications");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Your account is temporarily locked. Please try again later.");
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password.");
        }

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}