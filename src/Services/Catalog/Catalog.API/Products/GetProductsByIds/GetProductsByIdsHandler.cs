namespace Catalog.API.Products.GetProductsByIds
{
    public record GetProductsByIdsQuery(List<Guid> ProductIds) : IQuery<GetProductsByIdsResult>;

    public record GetProductsByIdsResult(List<Product> Products);

    internal class GetProductsByIdsQueryHandler
    (IDocumentSession session)
    : IQueryHandler<GetProductsByIdsQuery, GetProductsByIdsResult>
    {
        public async Task<GetProductsByIdsResult> Handle(GetProductsByIdsQuery query, CancellationToken cancellationToken)
        {
            var products = await session
                .Query<Product>()
                .Where(p => query.ProductIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            //if (!products.Any())
            //{
            //    _logger.LogWarning("No products found for provided ProductIds: {ProductIds}", string.Join(", ", query.ProductIds));
            //}

            var productList = products.ToList();

            return new GetProductsByIdsResult(productList);
        }
    }
}
