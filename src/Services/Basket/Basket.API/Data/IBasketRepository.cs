namespace Basket.API.Data;

public interface IBasketRepository
{
    Task<ShoppingCart> GetBasket(Guid UserId, CancellationToken cancellationToken = default);
    Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default);
    Task<bool> DeleteBasket(Guid UserId, CancellationToken cancellationToken = default);
}
