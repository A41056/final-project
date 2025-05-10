using BuildingBlocks.Messaging.Events;
using MassTransit;

namespace Catalog.API.Products.GetProductWithRating;

public record GetProductsWithRatingQuery(List<Guid> ProductIds) : IQuery<GetProductsWithRatingResult>;

public record GetProductsWithRatingResult(List<ProductWithRatingDto> Products);

internal class GetProductsWithRatingQueryHandler
    (IDocumentSession session, IBus bus, ILogger<GetProductsWithRatingQueryHandler> logger)
    : IQueryHandler<GetProductsWithRatingQuery, GetProductsWithRatingResult>
{
    public async Task<GetProductsWithRatingResult> Handle(GetProductsWithRatingQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Querying products with ratings for {Count} product IDs", query.ProductIds.Count);

        // Query products from Marten
        var products = await session
            .Query<Product>()
            .Where(p => query.ProductIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (!products.Any())
        {
            logger.LogWarning("No products found for provided ProductIds: {ProductIds}", string.Join(", ", query.ProductIds));
        }

        var client = bus.CreateRequestClient<ProductRequestRatingEvent>(
            new Uri("queue:product_rating_request_queue"),
            TimeSpan.FromSeconds(5)
        );

        var eventMessage = new ProductRequestRatingEvent
        {
            ProductIds = query.ProductIds
        };

        var response = await client.GetResponse<ProductRatingResponseEvent>(eventMessage, cancellationToken);

        var productDtos = products.Select(product =>
        {
            var rating = response.Message.Ratings.FirstOrDefault(r => r.ProductId == product.Id);
            return new ProductWithRatingDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageFiles = product.ImageFiles,
                IsHot = product.IsHot,
                IsActive = product.IsActive,
                Created = product.Created,
                Modified = product.Modified,
                CategoryIds = product.CategoryIds,
                Variants = product.Variants,
                AverageRating = rating?.AverageRating ?? 0
            };
        }).ToList();

        logger.LogInformation("Retrieved {Count} products with ratings", productDtos.Count);

        return new GetProductsWithRatingResult(productDtos);
    }
}