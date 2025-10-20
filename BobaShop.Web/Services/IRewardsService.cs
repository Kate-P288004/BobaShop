// ---------------------------------------------------------------
// File: Services/IRewardsService.cs
// Student: Kate Odabas (P288004)
// Purpose: Define rewards operations
// ---------------------------------------------------------------
using System.Threading.Tasks;

namespace BobaShop.Web.Services
{
    public interface IRewardsService
    {
        int CalculateEarnPoints(decimal orderSubtotal);
        decimal CalculateRedeemValue(int pointsToRedeem);
        int NormalizeRedeemRequest(int userPoints, int requestedPoints);
        Task AddPointsAsync(string userId, int points);
        Task DeductPointsAsync(string userId, int points);
    }
}
