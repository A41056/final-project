﻿using Ordering.Application.Orders.Queries.GetOrderStats;

namespace Ordering.API.Endpoints
{
    public record GetOrderStatsRequest(string Range = "7d");

    public class GetOrderStatsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/dashboard/order-stats", async ([AsParameters] GetOrderStatsRequest request, ISender sender) =>
            {
                var result = await sender.Send(new GetOrderStatsQuery(request.Range));
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .Produces<OrderStatsDto>(StatusCodes.Status200OK);
        }
    }
}
