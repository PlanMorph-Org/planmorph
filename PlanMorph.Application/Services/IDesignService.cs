using PlanMorph.Application.DTOs.Design;
using PlanMorph.Core.Entities;

namespace PlanMorph.Application.Services;

public interface IDesignService
{
    Task<IEnumerable<DesignDto>> GetApprovedDesignsAsync();
    Task<IEnumerable<DesignDto>> GetAllDesignsAsync();
    Task<DesignDto?> GetDesignByIdAsync(Guid id);
    Task<DesignDto?> CreateDesignAsync(CreateDesignDto createDesignDto, Guid architectId);
    Task<bool> ApproveDesignAsync(Guid designId);
    Task<bool> RejectDesignAsync(Guid designId);
    Task<IEnumerable<DesignDto>> GetDesignsByArchitectAsync(Guid architectId);
    Task<IEnumerable<DesignDto>> FilterDesignsAsync(DesignFilterDto filter);
}