namespace Catalog.API.Products.GetHomePageHotProduct;

public record GetTopHotProductsQuery(
    int? PageNumber = 1,
    int? PageSize = 10
) : IQuery<GetTopHotProductsResult>;

public record GetTopHotProductsResult(IEnumerable<Product> Products, int TotalItems);

internal class GetTopHotProductsQueryHandler(IDocumentSession session)
    : IQueryHandler<GetTopHotProductsQuery, GetTopHotProductsResult>
{
    public async Task<GetTopHotProductsResult> Handle(GetTopHotProductsQuery query, CancellationToken cancellationToken)
    {
        var baseQuery = session.Query<Product>()
            .Where(p => p.IsHot == true && p.IsActive == true);

        var totalItems = await baseQuery.CountAsync(cancellationToken);

        var products = await baseQuery
            .OrderByDescending(p => p.Created)
            .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

        return new GetTopHotProductsResult(products, totalItems);
    }
}