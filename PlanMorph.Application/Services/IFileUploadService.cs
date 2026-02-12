using Microsoft.AspNetCore.Http;
using PlanMorph.Core.Entities;

namespace PlanMorph.Application.Services;

public interface IFileUploadService
{
    Task<DesignFile> UploadDesignFileAsync(Guid designId, IFormFile file, FileCategory category);
    Task<List<DesignFile>> UploadMultipleFilesAsync(Guid designId, List<IFormFile> files, FileCategory category);
    Task<bool> DeleteFileAsync(Guid fileId);
    Task<string> GetSecureDownloadUrlAsync(Guid fileId, Guid userId);
    Task<string> GetAdminDownloadUrlAsync(Guid fileId);
}
