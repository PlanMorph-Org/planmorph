using PlanMorph.Core.Entities;

namespace PlanMorph.Application.Services;

public interface ICommissionService
{
    Task<CommissionQuote> GetDesignSaleCommissionAsync(decimal amountKes, User professional);
    Task<CommissionQuote> GetContractReferralCommissionAsync(decimal bqEstimatedContractValueKes);
}

public sealed class CommissionQuote
{
    public decimal RatePercent { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal NetAmount { get; set; }
}
