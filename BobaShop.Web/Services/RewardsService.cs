// ---------------------------------------------------------------
// File: Services/RewardsService.cs
// Student: Kate Odabas (P288004)
// Purpose: Rewards rules + update user balance
// ---------------------------------------------------------------
using System;
using System.Threading.Tasks;
using BobaShop.Web.Data; // your ApplicationDbContext namespace
using Microsoft.EntityFrameworkCore;

namespace BobaShop.Web.Services
{
    public class RewardsService : IRewardsService
    {
        private readonly ApplicationDbContext _db;

        private const int PointsPerDollar = 1;
        private const int RedeemBlock = 100;
        private const decimal RedeemBlockValue = 5m;

        public RewardsService(ApplicationDbContext db) => _db = db;

        public int CalculateEarnPoints(decimal orderSubtotal)
            => (int)Math.Floor(orderSubtotal * PointsPerDollar);

        public decimal CalculateRedeemValue(int pointsToRedeem)
            => (pointsToRedeem / RedeemBlock) * RedeemBlockValue;

        public int NormalizeRedeemRequest(int userPoints, int requestedPoints)
        {
            if (requestedPoints < RedeemBlock) return 0;
            var blocks = Math.Min(userPoints, requestedPoints) / RedeemBlock;
            return blocks * RedeemBlock;
        }

        public async Task AddPointsAsync(string userId, int points)
        {
            var user = await _db.Users.FirstAsync(u => u.Id == userId);
            user.RewardPoints += points;
            await _db.SaveChangesAsync();
        }

        public async Task DeductPointsAsync(string userId, int points)
        {
            var user = await _db.Users.FirstAsync(u => u.Id == userId);
            user.RewardPoints = Math.Max(0, user.RewardPoints - points);
            await _db.SaveChangesAsync();
        }
    }
}
