// -----------------------------------------------------------------------------
// File: Models/AdminDashboardVm.cs
// Project: BobaShop.Web 
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Defines the ViewModel used by the Admin Dashboard page in the BoBaTastic
//   web application. Combines user identity data, environment configuration,
//   and backend API connectivity metrics into a single model for rendering.
//   This class is consumed by the AdminController’s Index() action and the
//   corresponding Razor view (Views/Admin/Index.cshtml).
// -----------------------------------------------------------------------------

namespace BobaShop.Web.Models
{
    // -------------------------------------------------------------------------
    // Class: AdminDashboardVm
    // Purpose:
    //   Represents all data displayed on the administrator dashboard, including:
    //    Logged-in admin details (email, roles, points)
    //    Current hosting environment and app version
    //    API connectivity and product statistics
    //   Used to populate status panels and info cards on the Admin view.
    // -------------------------------------------------------------------------
    public class AdminDashboardVm
    {
        // -------------------------------------------------------------
        // Section: User Information
        // -------------------------------------------------------------

        // Administrator email address 
        public string Email { get; set; } = "";

        // Unique user ID (Identity primary key)
        public string UserId { get; set; } = "";

        // Roles assigned to the user (e.g., ["Admin"], ["Customer"])
        public List<string> Roles { get; set; } = new();

        // Loyalty or reward points for the user 
        public int RewardPoints { get; set; }

        // -------------------------------------------------------------
        // Section: Application & Environment Metadata
        // -------------------------------------------------------------

        // Current environment name (Development, Staging, Production)
        public string Environment { get; set; } = "";

        // Application version string (from appsettings.json)
        public string AppVersion { get; set; } = "";

        // Server’s local time 
        public DateTime ServerTime { get; set; }

        // -------------------------------------------------------------
        // Section: Backend API Status (Version 1)
        // -------------------------------------------------------------

        // Base URL of the connected BoBaTastic API (from configuration)
        public string ApiBaseUrl { get; set; } = "";

        // Indicates whether API connectivity test succeeded
        public bool ApiReachable { get; set; }

        // Count of drinks returned by API (via IDrinksApi.GetAllAsync)
        public int DrinksCount { get; set; }
    }
}
