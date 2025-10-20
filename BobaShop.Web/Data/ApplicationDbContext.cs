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
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
    }
}
