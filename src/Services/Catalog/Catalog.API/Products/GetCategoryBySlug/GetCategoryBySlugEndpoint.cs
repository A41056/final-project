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
                        string slug,
                        int? pageNumber,
                        int? pageSize,
                        string? search,
                        bool? isHot,
                        bool? isActive,
                        DateTime? createdFrom,
                        DateTime? createdTo,
                        ISender sender) =>
                                {
                                    var result = await sender.Send(new GetProductsByCategorySlugQuery(
                                        Slug: slug,
                                        PageNumber: pageNumber,
                                        PageSize: pageSize,
                                        Search: search,
                                        IsHot: isHot,
                                        IsActive: isActive,
                                        CreatedFrom: createdFrom,
                                        CreatedTo: createdTo
                                    ));

                                    return Results.Ok(result);
                                })
                    .WithName("GetProductsByCategorySlug")
                    .Produces<GetProductsResult>(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .WithSummary("Get products by category slug")
                    .WithDescription("Lấy danh sách sản phẩm theo slug của category");
        }
    }
}
