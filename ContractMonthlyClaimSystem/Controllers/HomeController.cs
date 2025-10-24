using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private static readonly List<TrackClaimViewModel> _trackedClaims = new()
        {
            new TrackClaimViewModel { UserId = 101, ModuleId = 501, Sessions = 10, Hours = 15, Status = "Pending",      SubmittedDateTime = new DateTime(2025, 9, 1, 8, 15, 0), DocumentName = "claim1.pdf" },
            new TrackClaimViewModel { UserId = 102, ModuleId = 502, Sessions = 8,  Hours = 12, Status = "Pre-approved", SubmittedDateTime = new DateTime(2025, 9, 2, 10, 0, 0), PreApprovedDateTime = new DateTime(2025, 9, 3, 12, 0, 0), DocumentName = "claim2.pdf" },
            new TrackClaimViewModel { UserId = 103, ModuleId = 503, Sessions = 12, Hours = 18, Status = "Approved",     SubmittedDateTime = new DateTime(2025, 9, 5, 14, 30, 0), PreApprovedDateTime = new DateTime(2025, 9, 6, 9, 0, 0), FinalizedDateTime = new DateTime(2025, 9, 7, 11, 0, 0), DocumentName = "claim3.pdf" }
        };

        private static string _loggedInUserRole = "";

        public IActionResult Login() { return View(); }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (model != null && !string.IsNullOrEmpty(model.Role))
            {
                string selectedRole = model.Role;
                if (selectedRole == "Lecturer") { _loggedInUserRole = "Lecturer"; return RedirectToAction("SubmitClaim"); }
                else if (selectedRole == "Programme Coordinator") { _loggedInUserRole = "Programme Coordinator"; return RedirectToAction("AdminDashboard"); }
                else if (selectedRole == "Academic Manager") { _loggedInUserRole = "Academic Manager"; return RedirectToAction("ManagerDashboard"); }
            }
            return View(model);
        }

        
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
           
            // We just redirect back to the login page to simulate success.
            return RedirectToAction("Login");
        }
       

        public IActionResult Logout() { _loggedInUserRole = ""; return RedirectToAction("Login"); }
        public IActionResult LecturerDashboard() { ViewData["UserRole"] = _loggedInUserRole; return View(_trackedClaims); }
        public IActionResult SubmitClaim() { ViewData["UserRole"] = _loggedInUserRole; return View(); }
        [HttpPost]
        public IActionResult SubmitClaim(ClaimViewModel newClaim) { return RedirectToAction("LecturerDashboard"); }
        public IActionResult AdminDashboard() { ViewData["UserRole"] = _loggedInUserRole; var pendingClaims = _trackedClaims.Where(c => c.Status == "Pending").Select(c => new ClaimViewModel { ClaimId = c.UserId, Description = $"Claim for Module {c.ModuleId}", SubmissionDate = c.SubmittedDateTime ?? DateTime.MinValue, Status = c.Status }); return View(pendingClaims); }
        public IActionResult ManagerDashboard() { ViewData["UserRole"] = _loggedInUserRole; var pendingClaims = _trackedClaims.Where(c => c.Status == "Pre-approved").Select(c => new ClaimViewModel { ClaimId = c.UserId, Description = $"Claim for Module {c.ModuleId}", SubmissionDate = c.SubmittedDateTime ?? DateTime.MinValue, Status = c.Status }); return View(pendingClaims); }
    }
}