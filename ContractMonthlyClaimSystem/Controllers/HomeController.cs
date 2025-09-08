using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private static readonly List<ClaimViewModel> _claims = new();

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid && model.Username != null)
            {
                string role = model.Username.ToLower();

                if (role == "lecturer")
                {
                    // --- THIS IS THE CHANGE ---
                    // If the user is a lecturer, redirect straight to the Submit Claim page.
                    return RedirectToAction("SubmitClaim");
                }
                else if (role == "coordinator" || role == "manager")
                {
                    return RedirectToAction("AdminDashboard");
                }
            }

            ModelState.AddModelError("", "Invalid username. Please try again.");
            return View(model);
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }

        public IActionResult Index()
        {
            return View(_claims.OrderByDescending(c => c.SubmissionDate).ToList());
        }

        public IActionResult SubmitClaim()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitClaim(ClaimViewModel newClaim)
        {
            if (ModelState.IsValid)
            {
                newClaim.ClaimId = _claims.Any() ? _claims.Max(c => c.ClaimId) + 1 : 1;
                newClaim.SubmissionDate = DateTime.Now;
                newClaim.Status = "Submitted";

                _claims.Add(newClaim);

                return RedirectToAction("Index");
            }
            return View(newClaim);
        }

        public IActionResult AdminDashboard()
        {
            var pendingClaims = _claims.Where(c => c.Status == "Submitted").ToList();
            return View(pendingClaims);
        }
    }
}