using Media.API.Service.Interfaces;

namespace Media.API.File.GetFileContentById;
public record GetFileContentByIdQuery(Guid Id) : IQuery<byte[]>;

internal class GetFileContentByIdQueryHandler(IDocumentSession session, IStorageService storageService)
    : IQueryHandler<GetFileContentByIdQuery, byte[]>
{
    public async Task<byte[]> Handle(GetFileContentByIdQuery query, CancellationToken cancellationToken)
    {
        var file = await session.LoadAsync<Model.File>(query.Id);
        if (file == null)
            throw new KeyNotFoundException($"File with ID {query.Id} not found.");

        return await storageService.GetBytes(file.StorageLocation);
    }
}
