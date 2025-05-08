namespace Review.API.Reviews.GetReviewByProductId;

public record GetReviewByProductRequest(int? PageNumber = 1, int? PageSize = 10);

public record GetReviewByProductResponse(IEnumerable<Models.Review> Reviews, int TotalItems);

public class GetReviewByProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/reviews/product/{productId}",
            async (Guid productId, [AsParameters] GetReviewByProductRequest request, ISender sender) =>
            {
                var query = new GetReviewByProductQuery(productId, request.PageNumber, request.PageSize);
                var result = await sender.Send(query);
                var response = result.Adapt<GetReviewByProductResponse>();
                return Results.Ok(response);
            })
            .WithName("GetReviewByProduct")
            .Produces<GetReviewByProductResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get Review By Product")
            .WithDescription("Get Review By Product with pagination");
    }
}
