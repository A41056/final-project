using Catalog.API.Products.GetProducts;
using Marten.Linq;

namespace Catalog.API.Products.GetCategoryBySlug
{
    public record GetProductsByCategorySlugQuery(
        string Slug,
        int? PageNumber = 1,
        int? PageSize = 10,
        string? Search = null,
        bool? IsHot = null,
        bool? IsActive = null,
        DateTime? CreatedFrom = null,
        DateTime? CreatedTo = null,
        decimal? PriceMin = null,
        decimal? PriceMax = null,
        List<string>? Tags = null,
        Dictionary<string, List<string>>? Properties = null
    ) : IQuery<GetProductsResult>;

    internal class GetProductsByCategorySlugQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductsByCategorySlugQuery, GetProductsResult>
    {
        public async Task<GetProductsResult> Handle(GetProductsByCategorySlugQuery query, CancellationToken cancellationToken)
        {
            var category = await session.Query<Category>()
                .FirstOrDefaultAsync(c => c.Slug == query.Slug, cancellationToken);

            if (category is null)
            {
                return new GetProductsResult([], 0);
            }

            var productQuery = session.Query<Product>()
                .Where(p => p.CategoryIds.Contains(category.Id));

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                productQuery = productQuery.Where(p =>
                    p.Name.Contains(query.Search) || p.Description.Contains(query.Search));
            }

            if (query.IsHot.HasValue)
            {
                productQuery = productQuery.Where(p => p.IsHot == query.IsHot);
            }

            if (query.IsActive.HasValue)
            {
                productQuery = productQuery.Where(p => p.IsActive == query.IsActive);
            }

            if (query.CreatedFrom.HasValue)
            {
                productQuery = productQuery.Where(p => p.Created >= query.CreatedFrom.Value);
            }

            if (query.CreatedTo.HasValue)
            {
                productQuery = productQuery.Where(p => p.Created <= query.CreatedTo.Value);
            }

            if (query.PriceMin.HasValue)
            {
                productQuery = productQuery.Where(p =>
                    p.Variants.Any(v => v.Price >= query.PriceMin.Value));
            }

            if (query.PriceMax.HasValue)
            {
                productQuery = productQuery.Where(p =>
                    p.Variants.Any(v => v.Price <= query.PriceMax.Value));
            }

            if (query.Tags is { Count: > 0 })
            {
                productQuery = productQuery.Where(p =>
                    p.Tags.Any(tag => query.Tags.Contains(tag)));
            }

            if (query.Properties is { Count: > 0 })
            {
                foreach (var (type, values) in query.Properties)
                {
                    productQuery = productQuery.Where(p =>
                        p.Variants.Any(v =>
                            v.Properties.Any(prop =>
                                prop.Type == type && values.Contains(prop.Value))));
                }
            }

            var totalItems = await productQuery.CountAsync(cancellationToken);
            var products = await productQuery
                .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

            return new GetProductsResult(products, totalItems);
        }
    }
}
