namespace Catalog.API.Products.GetRelatedProducts
{
    public record GetRelatedProductsQuery(
        Guid ProductId,
        int? PageNumber = 1,
        int? PageSize = 10
    ) : IQuery<GetRelatedProductsResult>;

    public record GetRelatedProductsResult(
        IEnumerable<Product> Products,
        int TotalItems
    );

    internal class GetRelatedProductsQueryHandler(IDocumentSession session)
        : IQueryHandler<GetRelatedProductsQuery, GetRelatedProductsResult>
    {
        public async Task<GetRelatedProductsResult> Handle(GetRelatedProductsQuery query, CancellationToken cancellationToken)
        {
            var currentProduct = await session.Query<Product>()
                .FirstOrDefaultAsync(p => p.Id == query.ProductId, cancellationToken);

            if (currentProduct == null)
            {
                return new GetRelatedProductsResult(Enumerable.Empty<Product>(), 0);
            }

            // First, get all products that match any category or tag
            var relatedQuery = session.Query<Product>()
                .Where(p => p.Id != query.ProductId && p.IsActive);

            // Execute the query to get the full list
            var allActiveProducts = await relatedQuery.ToListAsync(cancellationToken);

            // Then filter in memory
            var relatedProducts = allActiveProducts
                .Where(p =>
                    p.CategoryIds.Intersect(currentProduct.CategoryIds).Any() ||
                    p.Tags.Intersect(currentProduct.Tags).Any()
                )
                .OrderByDescending(p => p.IsHot)
                .ThenByDescending(p => p.Created);

            var totalItems = relatedProducts.Count();

            var pageSize = query.PageSize ?? 10;
            var pageNumber = query.PageNumber ?? 1;
            var skip = (pageNumber - 1) * pageSize;

            var pagedProducts = relatedProducts
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new GetRelatedProductsResult(pagedProducts, totalItems);
        }
    }
}