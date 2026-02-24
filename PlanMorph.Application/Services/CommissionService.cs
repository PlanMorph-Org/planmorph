using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Application.Services;

public class CommissionService : ICommissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public CommissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CommissionQuote> GetDesignSaleCommissionAsync(decimal amountKes, User professional)
    {
        if (professional.IsFoundingMember && (professional.Role == UserRole.Architect || professional.Role == UserRole.Engineer))
        {
            return new CommissionQuote
            {
                RatePercent = 0m,
                CommissionAmount = 0m,
                NetAmount = amountKes
            };
        }

        var rate = await ResolveRateAsync(CommissionRevenueType.DesignSale, amountKes);
        var commission = Math.Round(amountKes * rate / 100m, 2, MidpointRounding.AwayFromZero);

        return new CommissionQuote
        {
            RatePercent = rate,
            CommissionAmount = commission,
            NetAmount = amountKes - commission
        };
    }

    public async Task<CommissionQuote> GetContractReferralCommissionAsync(decimal bqEstimatedContractValueKes)
    {
        var rate = await ResolveRateAsync(CommissionRevenueType.ContractReferral, bqEstimatedContractValueKes);
        var commission = Math.Round(bqEstimatedContractValueKes * rate / 100m, 2, MidpointRounding.AwayFromZero);

        return new CommissionQuote
        {
            RatePercent = rate,
            CommissionAmount = commission,
            NetAmount = bqEstimatedContractValueKes - commission
        };
    }

    private async Task<decimal> ResolveRateAsync(CommissionRevenueType revenueType, decimal amountKes)
    {
        var tiers = await _unitOfWork.CommissionTiers.FindAsync(t => t.RevenueType == revenueType && t.IsActive);
        var tier = tiers
            .OrderBy(t => t.MinAmountKes)
            .FirstOrDefault(t => amountKes >= t.MinAmountKes && (t.MaxAmountKes == null || amountKes <= t.MaxAmountKes));

        if (tier != null)
            return tier.RatePercent;

        return GetDefaultRate(revenueType, amountKes);
    }

    private static decimal GetDefaultRate(CommissionRevenueType revenueType, decimal amountKes)
    {
        if (revenueType == CommissionRevenueType.DesignSale)
        {
            if (amountKes <= 20000m) return 3m;
            if (amountKes <= 50000m) return 4m;
            if (amountKes <= 100000m) return 5m;
            if (amountKes <= 200000m) return 6m;
            return 7m;
        }

        if (amountKes <= 500000m) return 1.5m;
        if (amountKes <= 2000000m) return 2m;
        if (amountKes <= 10000000m) return 2.5m;
        return 3m;
    }
}
