using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Review.API.EventHandlers;

public class ProductRequestRatingHandler : IConsumer<BuildingBlocks.Messaging.Events.ProductRequestRatingEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<ProductRequestRatingHandler> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPublishEndpoint _publishEndpoint;

    public ProductRequestRatingHandler(
        ISender sender,
        ILogger<ProductRequestRatingHandler> logger,
        IHttpClientFactory httpClientFactory,
        IPublishEndpoint publishEndpoint)
    {
        _sender = sender;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<BuildingBlocks.Messaging.Events.ProductRequestRatingEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received ProductRequestRatingEvent for {Count} product IDs", message.ProductIds.Count);

        try
        {
            // Send query to get average ratings
            var query = new GetAverageRatingsQuery(message.ProductIds);
            var ratings = await _sender.Send(query, context.CancellationToken);

            // Prepare response event
            var response = new ProductRatingResponseEvent
            {
                CorrelationId = message.Id,
                Ratings = ratings.Select(r => new ProductRatingData
                {
                    ProductId = r.ProductId,
                    AverageRating = r.AverageRating
                }).ToList()
            };

            // Publish response event
            await _publishEndpoint.Publish(response, context.CancellationToken);
            _logger.LogInformation("Published ProductRatingResponseEvent for {Count} product IDs", ratings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductRequestRatingEvent for {Count} product IDs", message.ProductIds.Count);
            throw;
        }
    }
}

// Query to get average ratings
public record GetAverageRatingsQuery(List<Guid> ProductIds) : IRequest<List<ProductRatingResult>>;

public record ProductRatingResult
{
    public Guid ProductId { get; init; }
    public double AverageRating { get; init; }
}

// Response event
public record ProductRatingResponseEvent : IntegrationEvent
{
    public Guid CorrelationId { get; init; }
    public List<ProductRatingData> Ratings { get; init; } = new();
}

public record ProductRatingData
{
    public Guid ProductId { get; init; }
    public double AverageRating { get; init; }
}

public class GetAverageRatingsQueryHandler : IRequestHandler<GetAverageRatingsQuery, List<ProductRatingResult>>
{
    private readonly IDocumentSession _session;
    private readonly ILogger<GetAverageRatingsQueryHandler> _logger;

    public GetAverageRatingsQueryHandler(
        IDocumentSession session,
        ILogger<GetAverageRatingsQueryHandler> logger)
    {
        _session = session;
        _logger = logger;
    }

    public async Task<List<ProductRatingResult>> Handle(GetAverageRatingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Querying average ratings for {Count} product IDs", request.ProductIds.Count);

        var ratings = await _session.Query<Models.Review>()
            .Where(r => r.IsActive && request.ProductIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new ProductRatingResult
            {
                ProductId = g.Key,
                AverageRating = g.Average(r => r.Rating)
            })
            .ToListAsync(cancellationToken);

        var result = request.ProductIds.Select(productId => ratings
            .FirstOrDefault(r => r.ProductId == productId) ?? new ProductRatingResult
            {
                ProductId = productId,
                AverageRating = 0
            })
            .ToList();

        _logger.LogInformation("Retrieved average ratings for {Count} product IDs", result.Count);
        return result;
    }
}