namespace PlanMorph.Core.Entities;

public class Design : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public double SquareFootage { get; set; }
    public int Stories { get; set; }
    public DesignCategory Category { get; set; }
    public DesignStatus Status { get; set; } = DesignStatus.PendingApproval;
    public decimal EstimatedConstructionCost { get; set; }
    
    // Architect who created this design
    public Guid ArchitectId { get; set; }
    public User Architect { get; set; } = null!;
    
    // Navigation properties
    public ICollection<DesignFile> Files { get; set; } = new List<DesignFile>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ModificationRequest> ModificationRequests { get; set; } = new List<ModificationRequest>();
    public ICollection<DesignVerification> Verifications { get; set; } = new List<DesignVerification>();
}

public enum DesignCategory
{
    Bungalow,
    TwoStory,
    Mansion,
    Apartment,
    Commercial
}

public enum DesignStatus
{
    Draft,
    PendingVerification,    // Waiting for architect/engineer verification
    PendingApproval,        // Verified, waiting for admin approval
    Approved,
    Rejected
}