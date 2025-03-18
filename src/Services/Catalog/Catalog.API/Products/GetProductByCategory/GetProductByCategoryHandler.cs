
namespace Catalog.API.Products.GetProductByCategory;

public record GetProductByCategoryQuery(Guid categoryId) : IQuery<GetProductByCategoryResult>;
public record GetProductByCategoryResult(IEnumerable<Product> Products);

internal class GetProductByCategoryQueryHandler
    (IDocumentSession session)
    : IQueryHandler<GetProductByCategoryQuery, GetProductByCategoryResult>
{
    public async Task<GetProductByCategoryResult> Handle(GetProductByCategoryQuery query, CancellationToken cancellationToken)
    {
        var products = await session.Query<Product>()
            .Where(p => p.CategoryIds.Contains(query.categoryId))
            .ToListAsync(cancellationToken);

        return new GetProductByCategoryResult(products);
    }
}
