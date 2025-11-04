// -----------------------------------------------------------------------------
// File: Data/ApplicationDbContext.cs
// Project: BobaShop.Web
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Defines the Entity Framework Core database context used by ASP.NET Core
//   Identity to manage user accounts, roles, and authentication data for the
//   BoBaTastic web application. The context is configured to use SQLite for
//   persistence and integrates with the custom ApplicationUser model.
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BobaShop.Web.Data
{
    // -------------------------------------------------------------------------
    // Class: ApplicationDbContext
    // Purpose:
    //   Provides EF Core access to ASP.NET Identity tables such as:
    //     AspNetUsers         – stores user login data 
    //     AspNetRoles         – defines roles 
    //     AspNetUserRoles     – links users to roles
    //     AspNetUserClaims    – additional user metadata
    //   Inherits from IdentityDbContext<ApplicationUser> to include the
    //   customised ApplicationUser class with RewardPoints and other fields.
    // -------------------------------------------------------------------------
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // ---------------------------------------------------------------------
        // Constructor:
        //   Accepts DbContextOptions injected at runtime (configured in Program.cs).
        // ---------------------------------------------------------------------
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

    }
}
