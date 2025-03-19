namespace Review.API.Reviews.GetReviewByProductId;

public record GetReviewByProductResponse(IEnumerable<Models.Review> Reviews);

public class GetReviewByProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/reviews/product/{product}", 
            async (Guid productId, ISender sender) =>
        {
            var result = await sender.Send(new GetReviewByProductQuery(productId));
            
            var response = result.Adapt<GetReviewByProductResponse>();
            
            return Results.Ok(response);
        })
        .WithName("GetReviewByProduct")
        .Produces<GetReviewByProductResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Review By Product")
        .WithDescription("Get Review By Product");
    }
}
