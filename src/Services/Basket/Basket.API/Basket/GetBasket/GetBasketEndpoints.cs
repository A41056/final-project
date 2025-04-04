namespace Basket.API.Basket.GetBasket;

//public record GetBasketRequest(string UserName); 
public record GetBasketResponse(ShoppingCart Cart);

public class GetBasketEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/basket/{userId}", async (Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new GetBasketQuery(userId));

            var respose = result.Adapt<GetBasketResponse>();

            return Results.Ok(respose);
        })
        .WithName("GetProductById")
        .Produces<GetBasketResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get Product By Id")
        .WithDescription("Get Product By Id");
    }
}
