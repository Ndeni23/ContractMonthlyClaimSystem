using ContractMonthlyClaimSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        // The list is now initialized as EMPTY. Claims will only be added
        // when the user submits the form.
        private static List<ClaimViewModel> _claims = new List<ClaimViewModel>();

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
                // To avoid an error on the first claim, we check if the list has any items.
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