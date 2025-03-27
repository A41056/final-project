namespace Media.API.FileType.GetFileTypes;

public record GetAllFileTypesQuery : IQuery<List<Model.FileType>>;

internal class GetAllFileTypesQueryHandler(IDocumentSession session)
    : IQueryHandler<GetAllFileTypesQuery, List<Model.FileType>>
{
    public async Task<List<Model.FileType>> Handle(GetAllFileTypesQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var fileTypes = await session.Query<Model.FileType>().ToListAsync(cancellationToken);
            return fileTypes.ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to retrieve file types", ex);
        }
    }
}