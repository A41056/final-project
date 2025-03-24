namespace Media.API.File.GetFileById;
public record GetFileByIdQuery(Guid Id) : IQuery<FileModel>;

internal class GetFileByIdQueryHandler(IDocumentSession session)
    : IQueryHandler<GetFileByIdQuery, FileModel>
{
    public async Task<FileModel> Handle(GetFileByIdQuery query, CancellationToken cancellationToken)
    {
        var file = await session.LoadAsync<Model.File>(query.Id);
        if (file == null)
            throw new KeyNotFoundException($"File with ID {query.Id} not found.");

        return new FileModel
        {
            Id = file.Id,
            FileName = file.FileName,
            StorageLocation = file.StorageLocation,
            FileSize = file.FileSize
        };
    }
}
