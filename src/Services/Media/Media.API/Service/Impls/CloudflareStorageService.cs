using Media.API.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel.Args;

namespace Media.API.Service.Impls;

public class CloudflareStorageService : IStorageService
{
    private readonly ILogger<CloudflareStorageService> _logger;
    private readonly string _bucketName;
    private readonly IMinioClient _minioClient;

    public CloudflareStorageService(ILogger<CloudflareStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;

        var cloudflareSettings = configuration.GetSection("CloudflareSetting").Get<CloudflareSetting>();

        _minioClient = new MinioClient()
            .WithEndpoint($"{cloudflareSettings.AccountId}.r2.cloudflarestorage.com")
            .WithCredentials(cloudflareSettings.AccessKey.Trim(), cloudflareSettings.SecretKey.Trim())
            .WithSSL()
            .Build();

        _bucketName = cloudflareSettings.BucketName;
    }

    public async Task<string> Upload(StorageInfo info)
    {
        try
        {
            info.Stream.Position = 0;
            var mimeType = GetMimeTypeFromExtension(info.FileName);

            using (var memoryStream = new MemoryStream())
            {
                await info.Stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var putArgs = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(info.FileName)
                    .WithStreamData(memoryStream)
                    .WithObjectSize(memoryStream.Length)
                    .WithContentType(mimeType);

                await _minioClient.PutObjectAsync(putArgs);
                _logger.LogInformation("Successfully uploaded {FileName} to bucket {BucketName}", info.FileName, _bucketName);
            }

            return info.FileName;
        }
        catch (Minio.Exceptions.MinioException minioEx)
        {
            _logger.LogError("MinIO Error - Message: {Message}, Response: {Response}",
                minioEx.Message, minioEx.Response);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError("Error uploading file: {Message}", e.Message);
            throw;
        }
    }

    public async Task DeleteFile(string fileName)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);

            await _minioClient.RemoveObjectAsync(args);
            _logger.LogInformation("Successfully deleted file {StorageLocation} from bucket {BucketName}",
                fileName, _bucketName);
        }
        catch (Exception e)
        {
            _logger.LogError("Error deleting file in Cloudflare R2: {Message}", e.Message);
            throw;
        }
    }

    public bool ExistsFileByPath(string path)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(path);

            var stat = _minioClient.StatObjectAsync(args).Result;
            return stat != null && stat.Size >= 0;
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
            using var memoryStream = new MemoryStream();
            var args = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithCallbackStream(async stream =>
                {
                    await stream.CopyToAsync(memoryStream);
                });

            await _minioClient.GetObjectAsync(args);
            return memoryStream.ToArray();
        }
        catch (Exception e)
        {
            _logger.LogError("Error getting bytes from Cloudflare R2: {Message}", e.Message);
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
    public string BucketName { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
}
