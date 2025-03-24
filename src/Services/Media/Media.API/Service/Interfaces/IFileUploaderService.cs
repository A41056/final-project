using Media.API.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Media.API.Service.Interfaces;

public interface IFileUploaderService
{
    Task<FileInsertModel> FileUploadAsync(IFormFile file, Guid itemId, EFileTypeIdentifier identifier);
    Task<List<ImageInfoModel>> CreateScaledImagesAsync(IFormFile file, Guid itemId, EFileTypeIdentifier identifier);
    Task<FileContentResult> DownLoadFileAsync(Guid id);
    Task<byte[]> GetFileContentByFileIdAsync(Guid fileId);
    Task DeleteFileAsync(string storageLocation);
}
