using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.API.Data;

public class CachedBasketRepository
    (IBasketRepository repository, IDistributedCache cache) 
    : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(Guid userId, CancellationToken cancellationToken = default)
    {
        var cachedBasket = await cache.GetStringAsync(userId.ToString(), cancellationToken);
        if (!string.IsNullOrEmpty(cachedBasket))
            return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket)!;

        var basket = await repository.GetBasket(userId, cancellationToken);
        await cache.SetStringAsync(userId.ToString(), JsonSerializer.Serialize(basket), cancellationToken);
        return basket;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        await repository.StoreBasket(basket, cancellationToken);

        await cache.SetStringAsync(basket.UserId.ToString(), JsonSerializer.Serialize(basket), cancellationToken);

        return basket;
    }

    public async Task<bool> DeleteBasket(Guid userId, CancellationToken cancellationToken = default)
    {
        await repository.DeleteBasket(userId, cancellationToken);

        await cache.RemoveAsync(userId.ToString(), cancellationToken);

        return true;
    }
}
