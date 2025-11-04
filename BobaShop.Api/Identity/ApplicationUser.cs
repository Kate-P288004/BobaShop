using Microsoft.AspNetCore.Identity;

namespace BobaShop.Api.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public int RewardPoints { get; set; } = 0;
    }
}
