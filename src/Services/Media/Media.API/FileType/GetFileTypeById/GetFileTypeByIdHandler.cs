namespace Media.API.FileType.GetFileTypeById;
public record GetFileTypeByIdQuery(Guid Id) : IQuery<Model.FileType>;
internal class GetFileTypeByIdQueryHandler(IDocumentSession session)
    : IQueryHandler<GetFileTypeByIdQuery, Model.FileType>
{
    public async Task<Model.FileType> Handle(GetFileTypeByIdQuery query, CancellationToken cancellationToken)
    {
        var fileType = await session.LoadAsync<Model.FileType>(query.Id);
        if (fileType == null)
            throw new KeyNotFoundException($"FileType with ID {query.Id} not found.");
        return fileType;
    }
}
