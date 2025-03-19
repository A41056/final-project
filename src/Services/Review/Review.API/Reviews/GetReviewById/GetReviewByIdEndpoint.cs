namespace Review.API.Reviews.GetReviewById;

//public record GetReviewByIdRequest();
public record GetReviewByIdResponse(Models.Review Review);

public class GetReviewByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/reviews/{id}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetReviewByIdQuery(id));

            var response = result.Adapt<GetReviewByIdResponse>();

            return Results.Ok(response);
        })
        .WithName("GetReviewById")
        .Produces<GetReviewByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Review By Id")
        .WithDescription("Get Review By Id");
    }
}
