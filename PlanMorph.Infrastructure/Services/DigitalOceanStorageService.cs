using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using PlanMorph.Core.Interfaces;
using System.Linq;

namespace PlanMorph.Infrastructure.Services;

public class DigitalOceanStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _endpoint;

    public DigitalOceanStorageService(IConfiguration configuration)
    {
        var accessKey = configuration["DigitalOcean:SpacesAccessKey"];
        var secretKey = configuration["DigitalOcean:SpacesSecretKey"];
        _endpoint = configuration["DigitalOcean:SpacesEndpoint"] ?? "";
        _bucketName = configuration["DigitalOcean:BucketName"] ?? "";

        var config = new AmazonS3Config
        {
            ServiceURL = _endpoint,
            ForcePathStyle = false // Use virtual-hosted-style URLs
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string contentType, bool isPublic = false)
    {
        var key = $"{folder}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
            CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private
        };

        await _s3Client.PutObjectAsync(request);

        // Return the full URL in virtual-hosted-style format
        // Format: https://bucketname.region.digitaloceanspaces.com/key
        var regionEndpoint = _endpoint.Replace("https://", "");
        var encodedKey = EncodeKeyForUrl(key);
        return $"https://{_bucketName}.{regionEndpoint}/{encodedKey}";
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var key = ExtractKeyFromUrl(fileUrl);
            
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetSignedUrlAsync(string fileUrl, int expiryMinutes = 60)
    {
        var key = ExtractKeyFromUrl(fileUrl);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };

        return await Task.FromResult(_s3Client.GetPreSignedURL(request));
    }

    public async Task<Stream> DownloadFileAsync(string fileUrl)
    {
        var key = ExtractKeyFromUrl(fileUrl);

        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(request);
        return response.ResponseStream;
    }

    private string ExtractKeyFromUrl(string fileUrl)
    {
        // Extract the key from the full URL
        // Example: https://bucketname.sgp1.digitaloceanspaces.com/folder/file.jpg -> folder/file.jpg
        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            var escapedUrl = fileUrl.Replace(" ", "%20");
            if (!Uri.TryCreate(escapedUrl, UriKind.Absolute, out uri))
            {
                throw new InvalidOperationException("Invalid file URL.");
            }
        }

        var unescapedPath = Uri.UnescapeDataString(uri.AbsolutePath);
        return unescapedPath.TrimStart('/');
    }

    private static string EncodeKeyForUrl(string key)
    {
        var segments = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("/", segments.Select(Uri.EscapeDataString));
    }
}
