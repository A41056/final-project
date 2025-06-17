namespace Catalog.API.Categories.GetCategories;

public record GetCategoriesQuery(int? PageNumber = 1, int? PageSize = 10) : IQuery<GetCategoriesResult>;
public record GetCategoriesResult(IEnumerable<Category> Categories);

public class GetCategoriesQueryHandler
    (IDocumentSession session)
    : IQueryHandler<GetCategoriesQuery, GetCategoriesResult>
{
    public async Task<GetCategoriesResult> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await session.Query<Category>()
            .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

        return new GetCategoriesResult(categories);
    }
}
