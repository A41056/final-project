namespace Ordering.Application.Orders.Queries.GetOrdersByName;
public class GetOrdersByIdHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetOrdersByIdQuery, GetOrdersByIdResult>
{
    public async Task<GetOrdersByIdResult> Handle(GetOrdersByIdQuery query, CancellationToken cancellationToken)
    {
        var orders = await dbContext.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .Where(o => o.Id == OrderId.Of(query.Id))
                .ToListAsync(cancellationToken);                

        return new GetOrdersByIdResult(orders.ToOrderDtoList());
    }    
}
