using Marten.Linq;

namespace Catalog.API.Products.GetProducts;
public record GetProductsQuery(
    int? PageNumber = 1,
    int? PageSize = 10,
    string? Search = null,
    Guid[]? CategoryIds = null,
    bool? IsHot = null,
    bool? IsActive = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null
) : IQuery<GetProductsResult>;

public record GetProductsResult(IEnumerable<Product> Products, int TotalItems);

internal class GetProductsQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var productQuery = session.Query<Product>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            productQuery = (IMartenQueryable<Product>)productQuery.Where(p => p.Name.Contains(query.Search) || p.Description.Contains(query.Search));
        }

        if (query.CategoryIds != null && query.CategoryIds.Any())
        {
            productQuery = (IMartenQueryable<Product>)productQuery.Where(p => p.CategoryIds.Any(cid => query.CategoryIds.Contains(cid)));
        }

        if (query.IsHot.HasValue)
        {
            productQuery = (IMartenQueryable<Product>)productQuery.Where(p => p.IsHot == query.IsHot.Value);
        }

        if (query.IsActive.HasValue)
        {
            productQuery = (IMartenQueryable<Product>)productQuery.Where(p => p.IsActive == query.IsActive.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            productQuery = (IMartenQueryable<Product>)productQuery.Where(p => p.Created >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            productQuery = (IMartenQueryable<Product>)productQuery.Where(p => p.Created <= query.CreatedTo.Value);
        }

        var totalItems = await productQuery.CountAsync(cancellationToken);

        var products = await productQuery
            .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

        return new GetProductsResult(products, totalItems);
    }
}