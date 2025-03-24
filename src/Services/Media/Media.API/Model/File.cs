namespace Media.API.Model;

public class File
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public string? StorageLocation { get; set; }
    public string? DisplayName { get; set; }
    public Guid FileTypeId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ProductId { get; set; }
    public string? ThumbnailStorageLocation { get; set; }
    public string? SmallStorageLocation { get; set; }
    public string? MediumStorageLocation { get; set; }
    public string? LargeStorageLocation { get; set; }
    public int ImageOrder { get; set; }
    public long FileSize { get; set; }
    public bool IsActive { get; set; }
    public DateTime? Created { get; set; }
}
