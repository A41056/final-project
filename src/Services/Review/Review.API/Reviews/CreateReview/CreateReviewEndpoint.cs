namespace Review.API.Reviews.CreateReview;

public record CreateReviewRequest(Guid ProductId, Guid UserId, int Rating, string Comment);

public record CreateReviewResponse(Guid Id);

public class CreateReviewEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reviews",
            async (CreateReviewRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateReviewCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<CreateReviewResponse>();

            return Results.Created($"/reviews/{response.Id}", response);

        })
        .WithName("CreateReview")
        .Produces<CreateReviewResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create Review")
        .WithDescription("Create Review");
    }
}
