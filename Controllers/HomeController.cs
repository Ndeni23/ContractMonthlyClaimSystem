using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.ViewModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() { return View(); }

        public IActionResult Login() { return View(); }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.Role))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u =>
                        u.Email.ToLower() == model.Email.ToLower() &&
                        u.Role == model.Role &&
                        u.Password == model.Password);

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
            catch (Exception) { return RedirectToAction("Error"); }
        }

        public IActionResult Register() { return View(); }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.Email))
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "An account with this email address already exists.");
                        return View(model);
                    }
                }
                var newUser = new User { FirstName = model.FirstName, LastName = model.LastName, Email = model.Email, Role = model.Role, Password = model.Password };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            catch (Exception) { return RedirectToAction("Error"); }
        }

        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Login"); }

        public async Task<IActionResult> LecturerDashboard()
        {
            try
            {
                ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
                var lecturerId = HttpContext.Session.GetInt32("UserId");
                if (lecturerId == null) { return RedirectToAction("Login"); }
                var userClaims = await _context.Claims.Include(c => c.SupportingDocuments).Where(c => c.LecturerId == lecturerId.Value).OrderByDescending(c => c.SubmissionDate).ToListAsync();
                return View(userClaims);
            }
            catch (Exception) { return RedirectToAction("Error"); }
        }

        public IActionResult SubmitClaim() { ViewData["UserRole"] = HttpContext.Session.GetString("UserRole"); return View(); }

        // --- THIS IS THE TEMPORARY DEBUGGING VERSION ---
        // The try-catch block has been removed to expose the real error.
        [HttpPost]
        public async Task<IActionResult> SubmitClaim(ClaimViewModel model, IFormFile supportingDocument)
        {
            if (!ModelState.IsValid)
            {
                ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
                return View(model);
            }

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

                // The error is likely happening on the next line. This will now cause a crash.
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("LecturerDashboard");
        }
        // --- END OF DEBUGGING VERSION ---

        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
                var pendingClaims = await _context.Claims.Include(c => c.Lecturer).Where(c => c.Status == "Submitted").ToListAsync();
                return View(pendingClaims);
            }
            catch (Exception) { return RedirectToAction("Error"); }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPreApproval(int claimId, string decision)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim != null) { claim.Status = decision; await _context.SaveChangesAsync(); }
                return RedirectToAction("AdminDashboard");
            }
            catch (Exception) { return RedirectToAction("Error"); }
        }

        public async Task<IActionResult> ManagerDashboard()
        {
            try
            {
                ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
                var preApprovedClaims = await _context.Claims.Include(c => c.Lecturer).Where(c => c.Status == "Pre-approved").ToListAsync();
                return View(preApprovedClaims);
            }
            catch (Exception) { return RedirectToAction("Error"); }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessFinalApproval(int claimId, string decision)
        {
            try
            {
                var claim = await _context.Claims.FindAsync(claimId);
                if (claim != null) { claim.Status = decision; await _context.SaveChangesAsync(); }
                return RedirectToAction("ManagerDashboard");
            }
            catch (Exception) { return RedirectToAction("Error"); }
        }
    }
}