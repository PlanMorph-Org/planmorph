using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Application.DTOs.Design;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Application.Services;

public class DesignService : IDesignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public DesignService(IUnitOfWork unitOfWork, IEmailService emailService, UserManager<User> userManager, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _userManager = userManager;
        _fileStorageService = fileStorageService;
    }

    public async Task<IEnumerable<DesignDto>> GetApprovedDesignsAsync()
    {
        var designs = await _unitOfWork.Designs.FindWithIncludesAsync(
            d => d.Status == DesignStatus.Approved,
            d => d.Files
        );
        var dtoTasks = designs.Select(MapToDtoAsync);
        return await Task.WhenAll(dtoTasks);
    }

    public async Task<IEnumerable<DesignDto>> GetAllDesignsAsync()
    {
        var designs = await _unitOfWork.Designs.FindWithIncludesAsync(
            d => true,
            d => d.Files
        );
        var dtoTasks = designs.Select(MapToDtoAsync);
        return await Task.WhenAll(dtoTasks);
    }

    public async Task<DesignDto?> GetDesignByIdAsync(Guid id)
    {
        var designs = await _unitOfWork.Designs.FindWithIncludesAsync(
            d => d.Id == id,
            d => d.Files
        );
        var design = designs.FirstOrDefault();
        return design == null ? null : await MapToDtoAsync(design);
    }

    public async Task<DesignDto?> CreateDesignAsync(CreateDesignDto createDesignDto, Guid architectId)
    {
        var design = new Design
        {
            Title = createDesignDto.Title,
            Description = createDesignDto.Description,
            Price = createDesignDto.Price,
            Bedrooms = createDesignDto.Bedrooms,
            Bathrooms = createDesignDto.Bathrooms,
            SquareFootage = createDesignDto.SquareFootage,
            Stories = createDesignDto.Stories,
            Category = createDesignDto.Category,
            EstimatedConstructionCost = createDesignDto.EstimatedConstructionCost,
            ArchitectId = architectId,
            Status = DesignStatus.PendingVerification
        };

        await _unitOfWork.Designs.AddAsync(design);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(design);
    }

    public async Task<bool> ApproveDesignAsync(Guid designId)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(designId);
        if (design == null) return false;
        if (design.Status != DesignStatus.PendingApproval) return false;

        design.Status = DesignStatus.Approved;
        design.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Designs.UpdateAsync(design);
        await _unitOfWork.SaveChangesAsync();

        // Send email notification to architect
        var architect = await _userManager.FindByIdAsync(design.ArchitectId.ToString());
        if (architect?.Email != null)
        {
            await _emailService.SendDesignApprovedEmailAsync(
                architect.Email,
                $"{architect.FirstName} {architect.LastName}",
                design.Title
            );
        }

        return true;
    }

    public async Task<bool> RejectDesignAsync(Guid designId)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(designId);
        if (design == null) return false;
        if (design.Status != DesignStatus.PendingApproval) return false;

        design.Status = DesignStatus.Rejected;
        design.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Designs.UpdateAsync(design);
        await _unitOfWork.SaveChangesAsync();

        // Send email notification to architect
        var architect = await _userManager.FindByIdAsync(design.ArchitectId.ToString());
        if (architect?.Email != null)
        {
            await _emailService.SendDesignRejectedEmailAsync(
                architect.Email,
                $"{architect.FirstName} {architect.LastName}",
                design.Title
            );
        }

        return true;
    }

    public async Task<IEnumerable<DesignDto>> GetDesignsByArchitectAsync(Guid architectId)
    {
        var designs = await _unitOfWork.Designs.FindWithIncludesAsync(
            d => d.ArchitectId == architectId,
            d => d.Files
        );
        var dtoTasks = designs.Select(MapToDtoAsync);
        return await Task.WhenAll(dtoTasks);
    }

    public async Task<IEnumerable<DesignDto>> FilterDesignsAsync(DesignFilterDto filter)
    {
        var designs = await _unitOfWork.Designs.FindWithIncludesAsync(
            d => d.Status == DesignStatus.Approved,
            d => d.Files
        );

        var query = designs.AsQueryable();

        if (filter.MinBedrooms.HasValue)
            query = query.Where(d => d.Bedrooms >= filter.MinBedrooms.Value);

        if (filter.MaxBedrooms.HasValue)
            query = query.Where(d => d.Bedrooms <= filter.MaxBedrooms.Value);

        if (filter.MinBathrooms.HasValue)
            query = query.Where(d => d.Bathrooms >= filter.MinBathrooms.Value);

        if (filter.MaxBathrooms.HasValue)
            query = query.Where(d => d.Bathrooms <= filter.MaxBathrooms.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(d => d.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(d => d.Price <= filter.MaxPrice.Value);

        if (filter.Category.HasValue)
            query = query.Where(d => d.Category == filter.Category.Value);

        if (filter.Stories.HasValue)
            query = query.Where(d => d.Stories == filter.Stories.Value);

        if (filter.MinSquareFootage.HasValue)
            query = query.Where(d => d.SquareFootage >= filter.MinSquareFootage.Value);

        if (filter.MaxSquareFootage.HasValue)
            query = query.Where(d => d.SquareFootage <= filter.MaxSquareFootage.Value);

        var filtered = query.ToList();
        var dtoTasks = filtered.Select(MapToDtoAsync);
        return await Task.WhenAll(dtoTasks);
    }

    private async Task<DesignDto> MapToDtoAsync(Design design)
    {
        var previewImages = await BuildPreviewUrlsAsync(design, FileCategory.PreviewImage);
        var previewVideos = await BuildPreviewUrlsAsync(design, FileCategory.PreviewVideo);

        return new DesignDto
        {
            Id = design.Id,
            Title = design.Title,
            Description = design.Description,
            Price = design.Price,
            Bedrooms = design.Bedrooms,
            Bathrooms = design.Bathrooms,
            SquareFootage = design.SquareFootage,
            Stories = design.Stories,
            Category = design.Category.ToString(),
            EstimatedConstructionCost = design.EstimatedConstructionCost,
            PreviewImages = previewImages,
            PreviewVideos = previewVideos
        };
    }

    private async Task<List<string>> BuildPreviewUrlsAsync(Design design, FileCategory category)
    {
        if (design.Files == null)
            return new List<string>();

        var previewFiles = design.Files
            .Where(f => f.Category == category)
            .ToList();

        if (previewFiles.Count == 0)
            return new List<string>();

        var urlTasks = previewFiles.Select(async file =>
        {
            try
            {
                // Use long-lived signed URLs for previews (up to 7 days).
                return await _fileStorageService.GetSignedUrlAsync(file.StorageUrl, 10080);
            }
            catch
            {
                return file.StorageUrl;
            }
        });

        return (await Task.WhenAll(urlTasks)).ToList();
    }
}
