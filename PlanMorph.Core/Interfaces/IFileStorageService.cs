namespace PlanMorph.Core.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string contentType, bool isPublic = false);
    Task<bool> DeleteFileAsync(string fileUrl);
    Task<string> GetSignedUrlAsync(string fileUrl, int expiryMinutes = 60);
    Task<Stream> DownloadFileAsync(string fileUrl);
}
