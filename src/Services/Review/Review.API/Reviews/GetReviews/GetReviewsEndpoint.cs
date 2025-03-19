namespace Review.API.Reviews.GetReviews;

public record GetReviewsRequest(int? PageNumber = 1, int? PageSize = 10);
public record GetReviewsResponse(IEnumerable<Models.Review> Reviews);

public class GetCategoriesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/reviews", async ([AsParameters] GetReviewsRequest request, ISender sender) =>
        {
            var query = request.Adapt<GetReviewsQuery>();

            var result = await sender.Send(query);

            var response = result.Adapt<GetReviewsResponse>();

            return Results.Ok(response);
        })
        .WithName("GetReviews")
        .Produces<GetReviewsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Reviews")
        .WithDescription("Get Reviews");
    }
}
