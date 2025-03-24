namespace Media.API.Service.Interfaces;

public interface IStorageService
{
    Task<string> Upload(StorageInfo info);
    Task DeleteFile(string fileName);
    Task<byte[]> GetBytes(string fileName);
    bool ExistsFileByPath(string path);
}
