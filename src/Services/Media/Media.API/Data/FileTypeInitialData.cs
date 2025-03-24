using Marten.Schema;
using Media.API.Enum;

namespace Media.API.Data;

public class FileTypeInitialData : IInitialData
{
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();

        if (await session.Query<Model.FileType>().AnyAsync(cancellation))
            return;

        var products = GetPreconfiguredFileTypes();
        session.Store(products.ToArray());
        await session.SaveChangesAsync(cancellation);
    }

    private static IEnumerable<Model.FileType> GetPreconfiguredFileTypes()
    {
        return new List<Model.FileType>
        {
            new Model.FileType
            {
                Id = Guid.NewGuid(),
                Name = "Product Image",
                Identifier = EFileTypeIdentifier.ImageProduct,
                DefaultStorageLocation = "media/images/{year}/{month}/{id}",
                FileExtensions = ".jpg,.jpeg,.png,.gif",
                FileNamePattern = "{id}_{userinput}_{timestamp}",
                MaxSize = 20 * 1024 * 1024, // 20MB
            },
            new Model.FileType
            {
                Id = Guid.NewGuid(),
                Name = "User Image",
                Identifier = EFileTypeIdentifier.ImageUser,
                DefaultStorageLocation = "media/images/{year}/{month}/{id}",
                FileExtensions = ".jpg,.jpeg,.png,.gif",
                FileNamePattern = "{id}_{userinput}_{timestamp}",
                MaxSize = 20 * 1024 * 1024, // 20MB
            },
            new Model.FileType
            {
                Id = Guid.NewGuid(),
                Name = "Product Video",
                Identifier = EFileTypeIdentifier.VideoProduct,
                DefaultStorageLocation = "media/videos/{year}/{month}/{id}",
                FileExtensions = ".mp4,.mov,.avi",
                FileNamePattern = "{id}_{userinput}_{timestamp}",
                MaxSize = 200 * 1024 * 1024, // 200MB
            }
        };
    }
}