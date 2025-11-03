namespace BobaShop.Web.Models
{
    public class AdminDashboardVm
    {
        // User
        public string Email { get; set; } = "";
        public string UserId { get; set; } = "";
        public List<string> Roles { get; set; } = new();
        public int RewardPoints { get; set; }

        // App/Env
        public string Environment { get; set; } = "";
        public string AppVersion { get; set; } = "";
        public DateTime ServerTime { get; set; }

        // API v1 status
        public string ApiBaseUrl { get; set; } = "";
        public bool ApiReachable { get; set; }
        public int DrinksCount { get; set; }
    }
}
