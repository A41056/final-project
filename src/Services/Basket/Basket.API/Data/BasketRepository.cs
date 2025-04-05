namespace Basket.API.Data;

public class BasketRepository(IDocumentSession session)
    : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(Guid userId, CancellationToken cancellationToken = default)
    {
        var basket = await session.LoadAsync<ShoppingCart>(userId, cancellationToken);
        
        return basket is null ? new ShoppingCart() : basket;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        session.Store(basket);
        await session.SaveChangesAsync(cancellationToken);
        return basket;
    }

    public async Task<bool> DeleteBasket(Guid userId, CancellationToken cancellationToken = default)
    {
        session.Delete<ShoppingCart>(userId);
        await session.SaveChangesAsync(cancellationToken);
        return true;
    }
}
