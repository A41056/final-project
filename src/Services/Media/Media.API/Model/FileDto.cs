using Microsoft.AspNetCore.Mvc;

namespace Media.API.Model;

public class FileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string DisplayName { get; set; }
    public string Extension { get; set; }
    public Guid FileTypeId { get; set; }
    public FileContentResult FileByteContent { get; set; }
}
