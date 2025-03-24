namespace Media.API.Model;

public class StorageInfo
{
    public string? StorageFolder { get; set; }
    public string? FileName { get; set; }
    public Stream? Stream { get; set; }
    public bool IsPublic { get; set; }
}
