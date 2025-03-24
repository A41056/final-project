using Media.API.Enum;

namespace Media.API.FileType.GetFileTypeByIdentifier;

public record GetFileTypeByIdentifierQuery(EFileTypeIdentifier Identifier) : IQuery<Model.FileType>;

internal class GetFileTypeByIdentifierQueryHandler(IDocumentSession session)
    : IQueryHandler<GetFileTypeByIdentifierQuery, Model.FileType>
{
    public async Task<Model.FileType> Handle(GetFileTypeByIdentifierQuery query, CancellationToken cancellationToken)
    {
        var fileType = await session.Query<Model.FileType>()
            .FirstOrDefaultAsync(ft => ft.Identifier == query.Identifier, cancellationToken);
        if (fileType == null)
            throw new KeyNotFoundException($"FileType with identifier {query.Identifier} not found.");
        return fileType;
    }
}

