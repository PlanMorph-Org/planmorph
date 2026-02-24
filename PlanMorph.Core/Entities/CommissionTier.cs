namespace PlanMorph.Core.Entities;

public class CommissionTier : BaseEntity
{
    public CommissionRevenueType RevenueType { get; set; }
    public decimal MinAmountKes { get; set; }
    public decimal? MaxAmountKes { get; set; }
    public decimal RatePercent { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum CommissionRevenueType
{
    DesignSale,
    ContractReferral
}
