namespace Catalog.API.Products.GetFilterOptions
{
    public record GetFilterOptionsQuery(string CategorySlug) : IQuery<FilterOptionsResult>;

    public record FilterOptionsResult(
        List<string> Tags,
        decimal MinPrice,
        decimal MaxPrice,
        Dictionary<string, List<string>> Properties
    );
    internal class GetFilterOptionsQueryHandler(IDocumentSession session)
    : IQueryHandler<GetFilterOptionsQuery, FilterOptionsResult>
    {
        public async Task<FilterOptionsResult> Handle(GetFilterOptionsQuery query, CancellationToken cancellationToken)
        {
            var category = await session.Query<Category>()
                .FirstOrDefaultAsync(c => c.Slug == query.CategorySlug, cancellationToken);

            if (category is null)
            {
                throw new Exception($"Category with slug '{query.CategorySlug}' not found");
            }

            var products = await session.Query<Product>()
                .Where(p => p.CategoryIds.Contains(category.Id))
                .ToListAsync(cancellationToken);

            var tags = products
                .SelectMany(p => p.Tags)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var prices = products
                .SelectMany(p => p.Variants)
                .Select(v => v.DiscountPrice)
                .ToList();

            var properties = new Dictionary<string, List<string>>();

            foreach (var variant in products.SelectMany(p => p.Variants))
            {
                foreach (var prop in variant.Properties)
                {
                    if (!properties.TryGetValue(prop.Type, out var values))
                    {
                        values = new List<string>();
                        properties[prop.Type] = values;
                    }

                    if (!values.Contains(prop.Value))
                    {
                        values.Add(prop.Value);
                    }
                }
            }

            foreach (var key in properties.Keys.ToList())
            {
                properties[key] = properties[key].Distinct().OrderBy(v => v).ToList();
            }

            var minPrice = prices.Count > 0 ? prices.Min() : 0;
            var maxPrice = prices.Count > 0 ? prices.Max() : 0;

            return new FilterOptionsResult(tags, minPrice.GetValueOrDefault(), maxPrice.GetValueOrDefault(), properties);
        }
    }
}
