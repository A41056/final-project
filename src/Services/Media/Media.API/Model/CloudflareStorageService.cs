using Amazon.S3;
using Amazon.S3.Model;
using Media.API.Service.Interfaces;

namespace Media.API.Model;

public class CloudflareStorageService : IStorageService
{
    private readonly ILogger<CloudflareStorageService> _logger;
    private readonly string _bucketName;
    private readonly AmazonS3Client _s3Client;

    public CloudflareStorageService(ILogger<CloudflareStorageService> logger, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _logger = logger;

        var cloudflareSettings = configuration.GetSection("CloudflareSetting").Get<CloudflareSetting>();

        _s3Client = new AmazonS3Client(
            cloudflareSettings.AccessKey,
            cloudflareSettings.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = $"https://{cloudflareSettings.AccountId}.r2.cloudflarestorage.com"
            });
        _bucketName = cloudflareSettings.BucketName;
    }

    public async Task<string> Upload(StorageInfo info)
    {
        try
        {
            info.Stream.Position = 0;

            var mimeType = GetMimeTypeFromExtension(info.FileName);

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = info.FileName,
                InputStream = info.Stream,
                ContentType = mimeType,
                DisablePayloadSigning = true
            };

            var response = await _s3Client.PutObjectAsync(request);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK && response.HttpStatusCode != System.Net.HttpStatusCode.Accepted)
            {
                throw new Exception("Upload to Cloudflare R2 failed");
            }

            return GenerateSignedUrl(info.FileName);
        }
        catch (Exception e)
        {
            _logger.LogError("Error uploading file to Cloudflare R2: " + e.Message);
            throw;
        }
    }

    public async Task DeleteFile(string fileName)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(_bucketName, fileName);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deleting file in Cloudflare R2: " + e.Message);
            throw;
        }
    }

    public bool ExistsFileByPath(string path)
    {
        try
        {
            var obj = _s3Client.GetObjectMetadataAsync(_bucketName, path).Result;
            return obj != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<byte[]> GetBytes(string fileName)
    {
        try
        {
            var obj = await _s3Client.GetObjectAsync(_bucketName, fileName);
            using var memoryStream = new MemoryStream();
            await obj.ResponseStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (Exception e)
        {
            _logger.LogError("Error getting bytes from Cloudflare R2: " + e.Message);
            throw;
        }
    }

    private static string GetMimeTypeFromExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLower();

        return extension switch
        {
            ".mp3" => "audio/mp3",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".doc" => "application/msword",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".svg" => "image/svg+xml",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }

    private string GenerateSignedUrl(string fileName)
    {
        var url = $"{fileName}";
        return url;
    }
}

public class CloudflareSetting
{
    public string AccountId { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public string BucketName { get; set; }
}
