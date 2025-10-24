using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public HomeController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Login() { return View(); }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (model.Email != null && model.Role != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower() && u.Role == model.Role);
                if (user != null)
                {
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    if (user.Role == "Lecturer") { return RedirectToAction("SubmitClaim"); }
                    else if (user.Role == "Programme Coordinator") { return RedirectToAction("AdminDashboard"); }
                    else if (user.Role == "Academic Manager") { return RedirectToAction("ManagerDashboard"); }
                }
            }
            ModelState.AddModelError("", "Invalid email, password, or role.");
            return View(model);
        }

        public IActionResult Register() { return View(); }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                return View(model);
            }
            var newUser = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Role = model.Role,
                Password = model.Password
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Login"); }

        public async Task<IActionResult> LecturerDashboard()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");

            // Get the ID of the currently logged-in lecturer
            var lecturerId = HttpContext.Session.GetInt32("UserId");

            // If no one is logged in, redirect to the login page
            if (lecturerId == null)
            {
                return RedirectToAction("Login");
            }

            // Fetch all claims from the database that belong to THIS lecturer.
            // Also include the related document data for each claim.
            var userClaims = await _context.Claims
                                        .Where(c => c.LecturerId == lecturerId.Value)
                                        .Include(c => c.SupportingDocuments) // Eagerly load the documents
                                        .OrderByDescending(c => c.SubmissionDate)
                                        .ToListAsync();

            return View(userClaims);
        }

        public IActionResult SubmitClaim()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(ClaimViewModel model, IFormFile supportingDocument)
        {
            var lecturerId = HttpContext.Session.GetInt32("UserId");
            if (lecturerId == null) { return RedirectToAction("Login"); }
            var newClaim = new Claim
            {
                LecturerId = lecturerId.Value,
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                TotalAmount = model.HoursWorked * model.HourlyRate,
                SubmissionDate = DateTime.Now,
                Status = "Submitted",
                Comments = model.Description
            };
            _context.Claims.Add(newClaim);
            await _context.SaveChangesAsync();
            if (supportingDocument != null && supportingDocument.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + supportingDocument.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await supportingDocument.CopyToAsync(fileStream);
                }
                var documentRecord = new SupportingDocument
                {
                    FileName = supportingDocument.FileName,
                    FilePath = uniqueFileName,
                    ClaimId = newClaim.ClaimId
                };
                _context.SupportingDocuments.Add(documentRecord);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("LecturerDashboard");
        }

        public async Task<IActionResult> AdminDashboard()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            var pendingClaims = await _context.Claims
                                        .Include(c => c.Lecturer)
                                        .Where(c => c.Status == "Submitted")
                                        .ToListAsync();
            return View(pendingClaims);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPreApproval(int claimId, string decision)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim != null)
            {
                claim.Status = decision;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AdminDashboard");
        }

        public async Task<IActionResult> ManagerDashboard()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            var preApprovedClaims = await _context.Claims
                                            .Include(c => c.Lecturer)
                                            .Where(c => c.Status == "Pre-approved")
                                            .ToListAsync();
            return View(preApprovedClaims);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessFinalApproval(int claimId, string decision)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim != null)
            {
                claim.Status = decision;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManagerDashboard");
        }
    }
}