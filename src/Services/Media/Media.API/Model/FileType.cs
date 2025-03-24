using Media.API.Enum;

namespace Media.API.Model;

public class FileType
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public EFileTypeIdentifier Identifier { get; set; }
    public string? DefaultStorageLocation { get; set; }
    public string? FileExtensions { get; set; }
    public string? FileNamePattern { get; set; }
    public long MaxSize { get; set; }
}
