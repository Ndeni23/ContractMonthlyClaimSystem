using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private static List<ClaimViewModel> _claims = new List<ClaimViewModel>
        {
            new ClaimViewModel { ClaimId = 1, SubmissionDate = DateTime.Now.AddDays(-30), Description = "August Teaching", Status = "Approved", TotalAmount = 5250.00m },
            new ClaimViewModel { ClaimId = 2, SubmissionDate = DateTime.Now.AddDays(-2), Description = "September Marking", Status = "Submitted", TotalAmount = 1800.00m }
        };

        public IActionResult Index()
        {
            // THIS IS THE FIX: We must pass the list of claims to the View.
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
                newClaim.ClaimId = _claims.Count + 1;
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