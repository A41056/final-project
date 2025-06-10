using Catalog.API.Services;

namespace Catalog.API.Products.SearchProduct
{
    public record SearchProductRequest(
        string? Query = null,
        string[]? Tags = null,
        string[]? CategoryIds = null,
        bool? IsHot = null,
        bool? IsActive = null,
        int? PageNumber = 1,
        int? PageSize = 10
    );

    public record SearchProductResponse(IEnumerable<ProductElasticModel> Products, int TotalItems);

    public class SearchProductEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/search", async ([AsParameters] SearchProductRequest request, IElasticSearchService esService) =>
            {
                var result = await esService.SearchProductsAsync(request);
                return Results.Ok(result);
            })
            .WithName("SearchProducts")
            .AllowAnonymous()
            .Produces<SearchProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Search Products by ElasticSearch")
            .WithDescription("Full-text search, filter by tags, category, status, pagination");
        }
    }
}
