using ImageMagick;
using Media.API.Enum;
using Media.API.File.GetFileById;
using Media.API.FileType.GetFileTypeByIdentifier;
using Media.API.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Media.API.Service.Impls;

public class FileUploaderService : IFileUploaderService
{
    private readonly ILogger<FileUploaderService> _logger;
    private readonly IStorageService _storageService;
    private readonly ISender _sender;

    public FileUploaderService(
        ILogger<FileUploaderService> logger,
        IStorageService storageService,
        ISender sender)
    {
        _logger = logger;
        _storageService = storageService;
        _sender = sender;
    }

    public async Task<FileInsertModel> FileUploadAsync(IFormFile file, Guid itemId, EFileTypeIdentifier identifier)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or null", nameof(file));

        _logger.LogInformation("Starting file upload for {FileName}", file.FileName);

        try
        {
            var fileTypeQuery = new GetFileTypeByIdentifierQuery(identifier);
            var fileType = await _sender.Send(fileTypeQuery);
            ValidateFile(file, fileType);

            string originFileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            string prefix = BuildStoragePrefix(fileType.DefaultStorageLocation, itemId);
            string fileName = BuildFileName(fileType.FileNamePattern, itemId, originFileName, extension);
            string fullKey = $"{prefix}/{fileName}";

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            var uri = await _storageService.Upload(new StorageInfo
            {
                FileName = fullKey,
                IsPublic = true,
                Stream = stream
            });

            return new FileInsertModel
            {
                FileName = fileName,
                DisplayName = originFileName,
                StorageLocation = uri,
                Extension = extension,
                FileSize = file.Length,
                FileTypeId = fileType.Id,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName}", file.FileName);
            throw new InvalidOperationException($"Failed to upload file {file.FileName}", ex);
        }
    }

    public async Task<List<ImageInfoModel>> CreateScaledImagesAsync(IFormFile file, Guid itemId, EFileTypeIdentifier identifier)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty or null", nameof(file));

        if (!Constants.FileSettings.ListImageType.Contains(identifier))
        {
            _logger.LogWarning("File type {Identifier} is not an image type, skipping resize", identifier);
            return new List<ImageInfoModel>();
        }

        _logger.LogInformation("Creating scaled images for {FileName}", file.FileName);

        try
        {
            var fileTypeQuery = new GetFileTypeByIdentifierQuery(identifier);
            var fileType = await _sender.Send(fileTypeQuery);
            ValidateFile(file, fileType);

            using var imageMagick = new MagickImage(file.OpenReadStream());
            int originalWidth = (int)imageMagick.Width;
            int originalHeight = (int)imageMagick.Height;

            var result = new List<ImageInfoModel>();
            var isGifFile = Path.GetExtension(file.FileName).Equals(".gif", StringComparison.OrdinalIgnoreCase);

            string prefix = BuildStoragePrefix(fileType.DefaultStorageLocation, itemId);
            string originFileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);

            if (isGifFile)
            {
                var imageInfo = await UploadScaledImageAsync(file, fileType, prefix, originFileName, extension, EImageSizeType.Original, originalWidth, originalHeight);
                result.Add(imageInfo);
            }
            else
            {
                foreach (EImageSizeType sizeType in System.Enum.GetValues(typeof(EImageSizeType)))
                {
                    var (height, width) = ScaledImageDimensions(sizeType, originalWidth, originalHeight);
                    if (originalHeight < height || originalWidth < width)
                        continue;

                    var imageInfo = await UploadScaledImageAsync(file, fileType, prefix, originFileName, extension, sizeType, width, height);
                    result.Add(imageInfo);
                }
            }

            _logger.LogInformation("Scaled images created successfully for {FileName}", file.FileName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create scaled images for {FileName}", file.FileName);
            throw new InvalidOperationException($"Failed to create scaled images for {file.FileName}", ex);
        }
    }

    public async Task<FileContentResult> DownLoadFileAsync(Guid id)
    {
        _logger.LogInformation("Downloading file with ID {FileId}", id);
        try
        {
            var query = new GetFileByIdQuery(id);
            var fileModel = await _sender.Send(query);

            var fileName = Path.GetFileName(fileModel.FileName);
            var contentType = GetMimeType(fileName);
            var content = await _storageService.GetBytes(fileModel.StorageLocation);

            _logger.LogInformation("File {FileId} downloaded successfully", id);
            return new FileContentResult(content, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file {FileId}", id);
            throw new InvalidOperationException($"Failed to download file {id}", ex);
        }
    }

    public async Task<byte[]> GetFileContentByFileIdAsync(Guid fileId)
    {
        _logger.LogInformation("Getting content for file {FileId}", fileId);
        try
        {
            var query = new GetFileByIdQuery(fileId);
            var fileModel = await _sender.Send(query);
            var content = await _storageService.GetBytes(fileModel.StorageLocation);

            _logger.LogInformation("Content retrieved for file {FileId}", fileId);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get content for file {FileId}", fileId);
            throw new InvalidOperationException($"Failed to get content for file {fileId}", ex);
        }
    }

    public async Task DeleteFileAsync(string storageLocation)
    {
        if (string.IsNullOrEmpty(storageLocation))
        {
            _logger.LogWarning("Storage location is null or empty, skipping delete operation.");
            return;
        }

        try
        {
            _logger.LogInformation("Deleting file from Cloudflare R2 at {StorageLocation}", storageLocation);
            await _storageService.DeleteFile(storageLocation);
            _logger.LogInformation("File at {StorageLocation} deleted successfully", storageLocation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file at {StorageLocation}", storageLocation);
            throw new InvalidOperationException($"Failed to delete file at {storageLocation}", ex);
        }
    }

    // Helper methods
    private async Task<ImageInfoModel> UploadScaledImageAsync(IFormFile file, Model.FileType fileType, string prefix,
        string originFileName, string extension, EImageSizeType sizeType, int width, int height)
    {
        Guid itemId = Guid.Parse(prefix.Split('/').Last());
        string fileName = BuildFileName(fileType.FileNamePattern, itemId, originFileName, extension, sizeType);
        string fullKey = $"{prefix}/{fileName}";

        using var stream = new MemoryStream();
        using (var imageMagick = new MagickImage(file.OpenReadStream()))
        {
            if (width != 0 && height != 0)
            {
                var size = new MagickGeometry((uint)width, (uint)height) { IgnoreAspectRatio = false };
                imageMagick.Resize(size);
            }
            await imageMagick.WriteAsync(stream);
        }

        stream.Position = 0;
        var uri = await _storageService.Upload(new StorageInfo
        {
            FileName = fullKey,
            IsPublic = true,
            Stream = stream
        });

        return new ImageInfoModel
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            DisplayName = originFileName,
            StorageLocation = uri,
            Extension = extension,
            FileSize = stream.Length,
            Uri = uri,
            SizeType = sizeType,
            FileTypeId = fileType.Id,
            ItemId = Guid.Parse(prefix.Split('/').Last()),
            IsPublic = true
        };
    }

    private static string BuildStoragePrefix(string template, Guid itemId)
    {
        var moment = DateTime.UtcNow;
        return template
            .Replace("{year}", moment.Year.ToString())
            .Replace("{month}", moment.Month.ToString("D2"))
            .Replace("{id}", itemId.ToString());
    }

    private static string BuildFileName(string pattern, Guid itemId, string userInput, string extension, EImageSizeType? sizeType = null)
    {
        string sizeSuffix = sizeType.HasValue && sizeType != EImageSizeType.Original
            ? $"_{sizeType.ToString().ToLower()}"
            : "";
        return pattern
            .Replace("{id}", itemId.ToString())
            .Replace("{userinput}", userInput.ConvertNonASCII() + sizeSuffix)
            .Replace("{timestamp}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()) + extension;
    }

    private static (int height, int width) ScaledImageDimensions(EImageSizeType itemSizeType, int originalWidth, int originalHeight)
    {
        int height, width;
        switch (itemSizeType)
        {
            case EImageSizeType.Large:
                width = 2048;
                height = 2048;
                break;
            case EImageSizeType.Medium:
                width = 1024;
                height = 1024;
                break;
            case EImageSizeType.Small:
                width = 512;
                height = 512;
                break;
            case EImageSizeType.Thumbnail:
                width = 266;
                height = 177;
                break;
            default:
                width = originalWidth;
                height = originalHeight;
                break;
        }
        return (height, width);
    }

    private void ValidateFile(IFormFile file, Model.FileType fileType)
    {
        if (file.Length > fileType.MaxSize)
            throw new ArgumentException($"File size exceeds maximum allowed size of {fileType.MaxSize} bytes.");
        var allowedExtensions = fileType.FileExtensions?.Split(',').Select(e => e.Trim().ToLower());
        var fileExtension = Path.GetExtension(file.FileName)?.ToLower();
        if (allowedExtensions != null && !allowedExtensions.Contains(fileExtension))
            throw new ArgumentException($"File extension {fileExtension} is not allowed for this FileType.");
    }

    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
