// -----------------------------------------------------------------------------
// File: Services/RewardsService.cs
// Project: BobaShop.Web (BoBaTastic Frontend)
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Implements business logic for the BoBaTastic rewards program. Responsible
//   for calculating points earned and redeemed, validating redemption requests,
//   and updating the user’s RewardPoints balance in the Identity database.
// -----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using BobaShop.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace BobaShop.Web.Services
{
    // -------------------------------------------------------------------------
    // Interface Dependency:
    //   IRewardsService defines the contract for rewards logic.
    //   This implementation provides methods for:
    //     Calculating earned points from order totals
    //      Determining redemption value
    //     Validating redemption requests
    //      Updating reward point balances in the database
    // -------------------------------------------------------------------------
    public class RewardsService : IRewardsService
    {
        private readonly ApplicationDbContext _db;

        // -------------------------------------------------------------
        // Reward Constants (Rules)
        // -------------------------------------------------------------
        private const int PointsPerDollar = 1;      // 1 point per $1 spent
        private const int RedeemBlock = 100;        // minimum redeemable block
        private const decimal RedeemBlockValue = 5m; // $5 discount per 100 points

        // Constructor
        public RewardsService(ApplicationDbContext db) => _db = db;

        // =====================================================================
        // Method: CalculateEarnPoints
        // Purpose:
        //   Converts an order subtotal (in AUD) into reward points based on
        //   the configured PointsPerDollar ratio.
        // Parameters:
        //   orderSubtotal – total of the order before redemption
        // Returns:
        //   Integer points earned for this purchase
        // Example:
        //   $18.70 order to 18 points
        // =====================================================================
        public int CalculateEarnPoints(decimal orderSubtotal)
            => (int)Math.Floor(orderSubtotal * PointsPerDollar);

        // =====================================================================
        // Method: CalculateRedeemValue
        // Purpose:
        //   Converts a number of redeemed points into a cash discount value.
        //   Redemption only applies in full 100-point blocks.
        // Parameters:
        //   pointsToRedeem – user’s requested points
        // Returns:
        //   Decimal discount value in AUD
        // Example:
        //   200 points to  $10.00 discount
        // =====================================================================
        public decimal CalculateRedeemValue(int pointsToRedeem)
            => (pointsToRedeem / RedeemBlock) * RedeemBlockValue;

        // =====================================================================
        // Method: NormalizeRedeemRequest
        // Purpose:
        //   Validates and normalizes the redemption request to ensure it:
        //     1. Meets the minimum redeem block threshold (100 pts)
        //     2. Does not exceed the user’s current available points
        // Returns:
        //   Adjusted redeemable amount (multiple of 100)
        // =====================================================================
        public int NormalizeRedeemRequest(int userPoints, int requestedPoints)
        {
            if (requestedPoints < RedeemBlock) return 0;
            var blocks = Math.Min(userPoints, requestedPoints) / RedeemBlock;
            return blocks * RedeemBlock;
        }

        // =====================================================================
        // Method: AddPointsAsync
        // Purpose:
        //   Adds reward points to the specified user account after purchase.
        //   Uses EF Core to update the ApplicationUser’s RewardPoints field.
        // Parameters:
        //   userId – Identity user’s unique ID
        //   points – number of points to add
        // =====================================================================
        public async Task AddPointsAsync(string userId, int points)
        {
            var user = await _db.Users.FirstAsync(u => u.Id == userId);
            user.RewardPoints += points;
            await _db.SaveChangesAsync();
        }

        // =====================================================================
        // Method: DeductPointsAsync
        // Purpose:
        //   Deducts redeemed points from the user’s balance, ensuring it does
        //   not fall below zero. Updates are persisted to the Identity DB.
        // Parameters:
        //   userId – Identity user’s unique ID
        //   points – number of points to deduct
        // =====================================================================
        public async Task DeductPointsAsync(string userId, int points)
        {
            var user = await _db.Users.FirstAsync(u => u.Id == userId);
            user.RewardPoints = Math.Max(0, user.RewardPoints - points);
            await _db.SaveChangesAsync();
        }
    }
}
