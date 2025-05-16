namespace Ordering.Application.Orders.Queries.GetOrdersByName;

public record GetOrdersByIdQuery(Guid Id)
    : IQuery<GetOrdersByIdResult>;

public record GetOrdersByIdResult(IEnumerable<OrderDto> Orders);