// ---------------------------------------------------------------
// File: Data/ApplicationDbContext.cs
// Student: Kate Odabas (P288004)
// Project: BoBaTastic – Web Identity DB Context
// Description: EF Core context for ASP.NET Identity (SQLite).
// ---------------------------------------------------------------
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BobaShop.Web.Data
{
    // IMPORTANT: Inherit from IdentityDbContext<ApplicationUser>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
    }
}
