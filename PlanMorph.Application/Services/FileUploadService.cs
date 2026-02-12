using Microsoft.AspNetCore.Http;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;

namespace PlanMorph.Application.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public FileUploadService(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<DesignFile> UploadDesignFileAsync(Guid designId, IFormFile file, FileCategory category)
    {
        // Validate file
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        // Determine file type
        var fileType = GetFileType(file.ContentType);
        
        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var folder = $"designs/{designId}/{category.ToString().ToLower()}";

        // Upload to storage
        string storageUrl;
        using (var stream = file.OpenReadStream())
        {
            var isPublic = category == FileCategory.PreviewImage || category == FileCategory.PreviewVideo;
            storageUrl = await _fileStorageService.UploadFileAsync(
                stream, 
                fileName, 
                folder, 
                file.ContentType,
                isPublic
            );
        }

        // Create database record
        var designFile = new DesignFile
        {
            DesignId = designId,
            FileName = file.FileName,
            FileType = fileType,
            Category = category,
            StorageUrl = storageUrl,
            FileSizeBytes = file.Length,
            IsWatermarked = category == FileCategory.PreviewImage || category == FileCategory.PreviewVideo
        };

        await _unitOfWork.DesignFiles.AddAsync(designFile);
        await _unitOfWork.SaveChangesAsync();

        await EnsureDesignVerificationsAsync(designId);

        return designFile;
    }

    public async Task<List<DesignFile>> UploadMultipleFilesAsync(Guid designId, List<IFormFile> files, FileCategory category)
    {
        var uploadedFiles = new List<DesignFile>();

        foreach (var file in files)
        {
            var designFile = await UploadDesignFileAsync(designId, file, category);
            uploadedFiles.Add(designFile);
        }

        return uploadedFiles;
    }

    public async Task<bool> DeleteFileAsync(Guid fileId)
    {
        var file = await _unitOfWork.DesignFiles.GetByIdAsync(fileId);
        if (file == null) return false;

        // Delete from storage
        await _fileStorageService.DeleteFileAsync(file.StorageUrl);

        // Delete from database
        await _unitOfWork.DesignFiles.DeleteAsync(file);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<string> GetSecureDownloadUrlAsync(Guid fileId, Guid userId)
    {
        var file = await _unitOfWork.DesignFiles.GetByIdAsync(fileId);
        if (file == null)
            throw new FileNotFoundException("File not found");

        // Check if user has purchased this design
        var hasPurchased = await _unitOfWork.Orders.ExistsAsync(o => 
            o.DesignId == file.DesignId && 
            o.ClientId == userId && 
            (o.Status == OrderStatus.Paid || o.Status == OrderStatus.Completed)
        );

        if (!hasPurchased)
            throw new UnauthorizedAccessException("User has not purchased this design");

        // Generate signed URL (expires in 1 hour)
        return await _fileStorageService.GetSignedUrlAsync(file.StorageUrl, 60);
    }

    public async Task<string> GetAdminDownloadUrlAsync(Guid fileId)
    {
        var file = await _unitOfWork.DesignFiles.GetByIdAsync(fileId);
        if (file == null)
            throw new FileNotFoundException("File not found");

        // Admin can access any file
        return await _fileStorageService.GetSignedUrlAsync(file.StorageUrl, 60);
    }

    private FileType GetFileType(string contentType)
    {
        return contentType.ToLower() switch
        {
            string ct when ct.Contains("image") => FileType.Image,
            string ct when ct.Contains("video") => FileType.Video,
            string ct when ct.Contains("pdf") => FileType.PDF,
            string ct when ct.Contains("dwg") || ct.Contains("dxf") => FileType.CAD,
            _ => throw new ArgumentException($"Unsupported file type: {contentType}")
        };
    }

    private async Task EnsureDesignVerificationsAsync(Guid designId)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(designId);
        if (design == null)
            return;

        var existingVerifications = await _unitOfWork.DesignVerifications.FindAsync(v => v.DesignId == designId);
        if (existingVerifications.Any())
            return;

        var files = await _unitOfWork.DesignFiles.FindAsync(f => f.DesignId == designId);
        bool hasArchitectural = files.Any(f => f.Category == FileCategory.ArchitecturalDrawing);
        bool hasStructural = files.Any(f => f.Category == FileCategory.StructuralDrawing);
        bool hasBoq = files.Any(f => f.Category == FileCategory.BOQ);

        if (!hasArchitectural || !hasStructural || !hasBoq)
            return;

        var verifications = new List<DesignVerification>
        {
            new() { DesignId = designId, VerificationType = VerificationType.Architectural },
            new() { DesignId = designId, VerificationType = VerificationType.Structural },
            new() { DesignId = designId, VerificationType = VerificationType.BOQArchitect },
            new() { DesignId = designId, VerificationType = VerificationType.BOQEngineer }
        };

        await _unitOfWork.DesignVerifications.AddRangeAsync(verifications);

        if (design.Status == DesignStatus.Draft)
        {
            design.Status = DesignStatus.PendingVerification;
            design.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Designs.UpdateAsync(design);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
