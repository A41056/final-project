using Catalog.API.Products.GetProducts;

namespace Catalog.API.Products.GetCategoryBySlug
{
    public record GetCategoryBySlugRequest(string Slug);

    public record GetCategoryBySlugResponse(Guid Id, string Name, string Slug);
    public class GetCategoryBySlugEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/products/category-slug/{slug}", async (
                    HttpRequest request,
                    string slug,
                    int? pageNumber,
                    int? pageSize,
                    string? search,
                    bool? isHot,
                    bool? isActive,
                    DateTime? createdFrom,
                    DateTime? createdTo,
                    decimal? priceMin,
                    decimal? priceMax,
                    ISender sender
                ) =>
            {
                var tags = request.Query["tags"].ToList();

                var properties = new Dictionary<string, List<string>>();
                foreach (var (key, value) in request.Query)
                {
                    if (key.StartsWith("properties["))
                    {
                        var type = key["properties[".Length..^1];
                        if (!properties.ContainsKey(type))
                            properties[type] = new List<string>();
                        properties[type].AddRange(value.ToList());
                    }
                }

                var result = await sender.Send(new GetProductsByCategorySlugQuery(
                    Slug: slug,
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                    Search: search,
                    IsHot: isHot,
                    IsActive: isActive,
                    CreatedFrom: createdFrom,
                    CreatedTo: createdTo,
                    PriceMin: priceMin,
                    PriceMax: priceMax,
                    Tags: tags,
                    Properties: properties
                ));

                return Results.Ok(result);
            });
        }
    }
}
