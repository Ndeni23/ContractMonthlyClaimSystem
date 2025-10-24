using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Hard-coded data for Track Claims page (to be replaced later) ---
        private static readonly List<TrackClaimViewModel> _trackedClaims = new()
        {
            new TrackClaimViewModel { UserId = 101, ModuleId = 501, Sessions = 10, Hours = 15, Status = "Pending",      SubmittedDateTime = new DateTime(2025, 9, 1, 8, 15, 0), DocumentName = "claim1.pdf" },
            new TrackClaimViewModel { UserId = 102, ModuleId = 502, Sessions = 8,  Hours = 12, Status = "Pre-approved", SubmittedDateTime = new DateTime(2025, 9, 2, 10, 0, 0), PreApprovedDateTime = new DateTime(2025, 9, 3, 12, 0, 0), DocumentName = "claim2.pdf" },
            new TrackClaimViewModel { UserId = 103, ModuleId = 503, Sessions = 12, Hours = 18, Status = "Approved",     SubmittedDateTime = new DateTime(2025, 9, 5, 14, 30, 0), PreApprovedDateTime = new DateTime(2025, 9, 6, 9, 0, 0), FinalizedDateTime = new DateTime(2025, 9, 7, 11, 0, 0), DocumentName = "claim3.pdf" }
        };

        // --- LOGIN, LOGOUT, REGISTER ---

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model.Email != null && model.Role != null)
            {
                // Find the user in the database by their email and role.
                // In a real app, you would also verify the hashed password.
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower() && u.Role == model.Role);

                if (user != null)
                {
                    // Store user's ID and Role in the session to "log them in"
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserRole", user.Role);

                    // Redirect based on role
                    if (user.Role == "Lecturer")
                    {
                        return RedirectToAction("SubmitClaim");
                    }
                    else if (user.Role == "Programme Coordinator")
                    {
                        return RedirectToAction("AdminDashboard");
                    }
                    else if (user.Role == "Academic Manager")
                    {
                        return RedirectToAction("ManagerDashboard");
                    }
                }
            }

            ModelState.AddModelError("", "Invalid email, password, or role.");
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Check if a user with this email already exists to prevent duplicates
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                return View(model);
            }

            // Create a new User object from the view model data
            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = model.Role,
                // IMPORTANT: In a real app, you would HASH this password. For this prototype, we store it as plain text.
                Password = model.Password
            };

            // Add the new user to the context and save it to the database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Redirect to the login page after successful registration
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            // Clear the session to log the user out
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // --- DASHBOARDS AND FORMS ---

        public IActionResult LecturerDashboard()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            // For now, this still shows hard-coded data. We will make this dynamic later.
            return View(_trackedClaims);
        }

        public IActionResult SubmitClaim()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(ClaimViewModel model, IFormFile supportingDocument)
        {
            // Get the ID of the currently logged-in user from the session
            var lecturerId = HttpContext.Session.GetInt32("UserId");

            // Security check: If no one is logged in, redirect them to the login page
            if (lecturerId == null)
            {
                return RedirectToAction("Login");
            }

            // Create a new Claim DATABASE model from the form's view model data
            var newClaim = new Claim
            {
                LecturerId = lecturerId.Value,
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                TotalAmount = model.HoursWorked * model.HourlyRate, // Calculate the total
                SubmissionDate = DateTime.Now,
                Status = "Submitted", // Set the initial status
                Comments = model.Description // Using the description field for comments/notes
            };

            // Add the new claim to the context and save it to the database
            _context.Claims.Add(newClaim);
            await _context.SaveChangesAsync();

            // After saving, redirect the lecturer to their dashboard to see the new claim
            return RedirectToAction("LecturerDashboard");
        }
    }
}