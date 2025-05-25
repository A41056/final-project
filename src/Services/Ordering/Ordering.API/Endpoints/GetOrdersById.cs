using Ordering.Application.Orders.Queries.GetOrdersByName;

namespace Ordering.API.Endpoints;

public record GetOrdersByIdResponse(IEnumerable<OrderDto> Orders);

public class GetOrdersById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/orders/{orderId}", async (Guid orderId, ISender sender) =>
        {
            var result = await sender.Send(new GetOrdersByIdQuery(orderId));

            var response = result.Adapt<GetOrdersByIdResponse>();

            return Results.Ok(response);
        })
        .WithName("GetOrdersById")
        .RequireAuthorization()
        .Produces<GetOrdersByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get Orders By ID")
        .WithDescription("Get Orders By ID");
    }
}
