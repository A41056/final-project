using Media.API.Enum;
using Microsoft.AspNetCore.StaticFiles;

namespace Media.API.Helper;

public static class FileHelper
{
    public static void IsValidFile(IFormFile file, Model.FileType filetype)
    {
        IsValidFile(file.Length, file.FileName, filetype);
    }
    public static void IsValidFile(long fileLength, string fileName, Model.FileType filetype)
    {
        if (fileLength == 0) throw new Media.API.Exceptions.ValidationException(string.Concat(Constants.FileSettings.FileIsEmptyKey, nameof(fileLength)));

        if (filetype == null) throw new ValidationException(string.Concat(Constants.FileSettings.FileTypeIsNotConfiguredKey, nameof(filetype)));

        if (string.IsNullOrEmpty(filetype.FileExtensions)) throw new ValidationException(string.Concat(Constants.FileSettings.FileTypeIsNotConfiguredKey, nameof(Model.FileType.FileExtensions)));

        string extension = Path.GetExtension(fileName)?.ToLower().Trim();
        string[] extensionConfigs = filetype.FileExtensions.Split(',').Select(e => e.ToLower().Trim()).ToArray();

        if (!extensionConfigs.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ValidationException(string.Concat(string.Format(Constants.FileSettings.FileExtensionNotValid, filetype.Name, string.Join("_", extensionConfigs))
                .Replace(" ", "_"), nameof(extension)));
        }

        // Megabyte unit
        var maxSizeByte = filetype.MaxSize * 1024 * 1024;
        if (fileLength > maxSizeByte)
        {
            throw new ValidationException(string.Concat(string.Format(Constants.FileSettings.FileSizeNotValid, filetype.Name, filetype.MaxSize)
                .Replace(" ", "_"), nameof(maxSizeByte)));
        }
    }

    public static string GetMimeType(string fileName)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName, out string contentType))
        {
            contentType = "application/octet-stream";
        }

        return contentType;
    }

    public static bool ValidateFileFromUrl(string url, ETypeOfFile eTypeOfFile)
    {
        var dicExtension = new Dictionary<ETypeOfFile, string>()
            {
                { ETypeOfFile.Image, ".jpg;.jpeg;.jpe;.png;.svg" },
                { ETypeOfFile.Audio, ".mp3" },
                { ETypeOfFile.Video, ".mp4;.avi;.mov;.wmv" },
                { ETypeOfFile.Pdf, ".pdf" },
                { ETypeOfFile.Script, ".txt;.srt" },
            };

        string fileExtension = Path.GetExtension(url);
        string[] extensions = dicExtension[eTypeOfFile].Split(';');

        return extensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
    }

    public static string UpdateLastSegmentToLower(string path)
    {
        var segments = path.Split('/');
        if (segments.Length == 0)
        {
            return path;
        }

        segments[^1] = Uri.EscapeDataString(segments[^1]);

        return string.Join("/", segments);
    }
}
