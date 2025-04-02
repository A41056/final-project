using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Catalog.API.EventHandlers;

public class ProductRatingUpdatedEventHandler : IConsumer<ProductRatingUpdatedEvent>
{
    private readonly IDocumentSession _session;

    public ProductRatingUpdatedEventHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task Consume(ConsumeContext<ProductRatingUpdatedEvent> context)
    {
        var message = context.Message;
        var product = await _session.LoadAsync<ProductWithRatingDto>(message.ProductId);

        if (product != null)
        {
            product.AverageRating = message.NewAverageRating;
            product.Modified = DateTime.UtcNow;
            await _session.SaveChangesAsync();
        }
    }
}
