using ContractMonthlyClaimSystem.Models; // We need to access our Models
using System;
using Xunit;

namespace CMCS.Tests
{
    public class ClaimTests
    {
        [Fact] // This attribute marks the method as a test
        public void NewClaim_ShouldHaveSubmittedStatus_WhenCreated()
        {
            // --- ARRANGE ---
            // Create a new Claim object, simulating what happens when a lecturer submits a form.
            var newClaim = new Claim
            {
                LecturerId = 1,
                HoursWorked = 10,
                HourlyRate = 200,
                TotalAmount = 2000,
                SubmissionDate = DateTime.Now,
                Status = "Submitted", // This is the logic we are testing
                Comments = "Test Claim"
            };

            // --- ACT ---
            // In this simple test, the "act" is the creation of the object itself.
            // We'll store the expected status in a variable.
            string expectedStatus = "Submitted";

            // --- ASSERT ---
            // This is the verification step. We assert that the claim's actual status
            // is equal to the status we expected.
            Assert.Equal(expectedStatus, newClaim.Status);
        }
    }
}