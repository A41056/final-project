namespace Review.API.Reviews.GetReviewByProductId;

public record GetReviewByProductQuery(Guid productId) : IQuery<GetReviewByProductResult>;
public record GetReviewByProductResult(IEnumerable<Models.Review> Reviews);

internal class GetReviewByProductQueryHandler
    (IDocumentSession session)
    : IQueryHandler<GetReviewByProductQuery, GetReviewByProductResult>
{
    public async Task<GetReviewByProductResult> Handle(GetReviewByProductQuery query, CancellationToken cancellationToken)
    {
        var products = await session.Query<Models.Review>()
            .Where(p => p.ProductId == query.productId)
            .ToListAsync(cancellationToken);

        return new GetReviewByProductResult(products);
    }
}
