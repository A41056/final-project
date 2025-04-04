namespace Basket.API.Basket.GetBasket;

public record GetBasketQuery(Guid UserId) : IQuery<GetBasketResult>;
public record GetBasketResult(ShoppingCart Cart);

public class GetBasketQueryHandler(IBasketRepository repository)
    : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public async Task<GetBasketResult> Handle(GetBasketQuery query, CancellationToken cancellationToken)
    {
        var basket = await repository.GetBasket(query.UserId);

        return new GetBasketResult(basket);
    }
}
