using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.ViewModel; // Kept singular to match your project
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

        // --- AUTHENTICATION ---

        public IActionResult Login() { return View(); }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (model.Email != null && model.Role != null)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower() && u.Role == model.Role);

                    if (user != null && user.Password == model.Password)
                    {
                        HttpContext.Session.SetInt32("UserId", user.UserId);
                        HttpContext.Session.SetString("UserRole", user.Role);

                        if (user.Role == "Lecturer") return RedirectToAction("SubmitClaim");
                        if (user.Role == "Programme Coordinator") return RedirectToAction("AdminDashboard");
                        if (user.Role == "Academic Manager") return RedirectToAction("ManagerDashboard");
                        if (user.Role == "HR") return RedirectToAction("HRDashboard");
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
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email address already exists.");
                    return View(model);
                }
                var newUser = new User { FirstName = model.FirstName, LastName = model.LastName, Email = model.Email, Role = model.Role, Password = model.Password };
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            catch (Exception) { return RedirectToAction("Error"); }
        }

        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Login"); }

        // --- LECTURER FEATURES ---

        public async Task<IActionResult> LecturerDashboard()
        {
            try
            {
                ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
                var lecturerId = HttpContext.Session.GetInt32("UserId");
                if (lecturerId == null) { return RedirectToAction("Login"); }

                var userClaims = await _context.Claims
                    .Include(c => c.SupportingDocuments)
                    .Where(c => c.LecturerId == lecturerId.Value)
                    .OrderByDescending(c => c.SubmissionDate)
                    .ToListAsync();

                return View(userClaims);
            }
            catch (Exception) { return RedirectToAction("Error"); }
        }

        public IActionResult SubmitClaim() { ViewData["UserRole"] = HttpContext.Session.GetString("UserRole"); return View(); }

        [HttpPost]
        public async Task<IActionResult> SubmitClaim(ClaimViewModel model, IFormFile supportingDocument)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
                    return View(model);
                }

                var lecturerId = HttpContext.Session.GetInt32("UserId");
                if (lecturerId == null) { return RedirectToAction("Login"); }

                // AUTOMATION LOGIC
                string status = "Submitted";
                string comments = model.Description;

                if (model.HoursWorked > 100)
                {
                    status = "Rejected";
                    comments += " [System Auto-Reject: Hours exceed limit]";
                }
                else if (model.HourlyRate < 50)
                {
                    status = "Rejected";
                    comments += " [System Auto-Reject: Rate below minimum wage]";
                }

                var newClaim = new Claim
                {
                    LecturerId = lecturerId.Value,
                    HoursWorked = model.HoursWorked,
                    HourlyRate = model.HourlyRate,
                    TotalAmount = model.HoursWorked * model.HourlyRate,
                    SubmissionDate = DateTime.Now,
                    Status = status,
                    Comments = comments
                };
                _context.Claims.Add(newClaim);
                await _context.SaveChangesAsync();

                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    string webRootPath = _hostEnvironment.WebRootPath;
                    if (string.IsNullOrEmpty(webRootPath)) webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    string uploadsFolder = Path.Combine(webRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(supportingDocument.FileName);
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
            catch (Exception) { return RedirectToAction("Error"); }
        }

        // --- APPROVAL WORKFLOW ---

        public async Task<IActionResult> AdminDashboard()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            var pendingClaims = await _context.Claims.Include(c => c.Lecturer).Where(c => c.Status == "Submitted").ToListAsync();
            return View(pendingClaims);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPreApproval(int claimId, string decision)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim != null) { claim.Status = decision; await _context.SaveChangesAsync(); }
            return RedirectToAction("AdminDashboard");
        }

        public async Task<IActionResult> ManagerDashboard()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            var preApprovedClaims = await _context.Claims.Include(c => c.Lecturer).Where(c => c.Status == "Pre-approved").ToListAsync();
            return View(preApprovedClaims);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessFinalApproval(int claimId, string decision)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim != null) { claim.Status = decision; await _context.SaveChangesAsync(); }
            return RedirectToAction("ManagerDashboard");
        }

        // --- HR REPORTING ---
        public async Task<IActionResult> HRDashboard()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            var approvedClaims = await _context.Claims.Include(c => c.Lecturer).Where(c => c.Status == "Approved").ToListAsync();
            return View(approvedClaims);
        }

        // --- PROFILE MANAGEMENT (NEW FOR PART 3) ---
        public async Task<IActionResult> Profile()
        {
            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(userId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User updatedUser)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Email = updatedUser.Email;
                user.Password = updatedUser.Password; // Allows password update

                await _context.SaveChangesAsync();
                ViewData["Message"] = "Profile updated successfully!";
            }

            ViewData["UserRole"] = HttpContext.Session.GetString("UserRole");
            return View("Profile", user);
        }
    }
}