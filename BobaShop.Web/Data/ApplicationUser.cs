// ---------------------------------------------------------------
// File: Data/ApplicationUser.cs
// Student: Kate Odabas (P288004)
// Purpose: Identity user with Rewards fields
// ---------------------------------------------------------------
using Microsoft.AspNetCore.Identity;

namespace BobaShop.Web.Data
{
    public class ApplicationUser : IdentityUser
    {
        public int RewardPoints { get; set; } = 0;
        public string? FullName { get; set; }
    }
}
