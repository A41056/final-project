namespace Catalog.API.Products.GetProductStats
{
    public record GetProductStatsQuery() : IQuery<ProductStatsDto>;

    internal class GetProductStatsQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductStatsQuery, ProductStatsDto>
    {
        public async Task<ProductStatsDto> Handle(GetProductStatsQuery query, CancellationToken ct)
        {
            var products = await session.Query<Product>().ToListAsync(ct);

            var total = products.Count;
            var active = products.Count(p => p.IsActive);
            var hot = products.Count(p => p.IsHot);
            var outOfStock = products.Count(p =>
                p.Variants.Any() && p.Variants.All(v => v.StockCount <= 0)
            );

            return new ProductStatsDto(total, active, hot, outOfStock);
        }
    }
}
