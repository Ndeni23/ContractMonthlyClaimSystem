using ContractMonthlyClaimSystem.Controllers;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore; // <-- Ensures EF Core is referenced

namespace ContractMonthlyClaimSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }
    }
}