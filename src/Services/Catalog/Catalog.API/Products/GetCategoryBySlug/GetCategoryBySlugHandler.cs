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
    DateTime? CreatedTo = null
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

            var productQuery = session.Query<Product>();

            productQuery = (IMartenQueryable<Product>)productQuery
                .Where(p => p.CategoryIds.Contains(category.Id));

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                productQuery = (IMartenQueryable<Product>)productQuery
                    .Where(p => p.Name.Contains(query.Search) || p.Description.Contains(query.Search));
            }

            if (query.IsHot.HasValue)
            {
                productQuery = (IMartenQueryable<Product>)productQuery
                    .Where(p => p.IsHot == query.IsHot.Value);
            }

            if (query.IsActive.HasValue)
            {
                productQuery = (IMartenQueryable<Product>)productQuery
                    .Where(p => p.IsActive == query.IsActive.Value);
            }

            if (query.CreatedFrom.HasValue)
            {
                productQuery = (IMartenQueryable<Product>)productQuery
                    .Where(p => p.Created >= DateTime.SpecifyKind(query.CreatedFrom.Value, DateTimeKind.Unspecified));
            }

            if (query.CreatedTo.HasValue)
            {
                productQuery = (IMartenQueryable<Product>)productQuery
                    .Where(p => p.Created <= DateTime.SpecifyKind(query.CreatedTo.Value, DateTimeKind.Unspecified));
            }

            var totalItems = await productQuery.CountAsync(cancellationToken);

            var products = await productQuery
                .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

            return new GetProductsResult(products, totalItems);
        }
    }
}
