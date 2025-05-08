using BuildingBlocks.Pagination;
using BuildingBlocks.CQRS;
using Marten;

namespace Review.API.Reviews.GetReviewByProductId;

public record GetReviewByProductQuery(Guid ProductId, int? PageNumber = 1, int? PageSize = 10)
    : IQuery<GetReviewByProductResult>;

public record GetReviewByProductResult(IEnumerable<Models.Review> Reviews, int TotalItems);

internal class GetReviewByProductQueryHandler(IDocumentSession session)
    : IQueryHandler<GetReviewByProductQuery, GetReviewByProductResult>
{
    public async Task<GetReviewByProductResult> Handle(GetReviewByProductQuery query, CancellationToken cancellationToken)
    {
        var reviewQuery = session.Query<Models.Review>()
            .Where(p => p.ProductId == query.ProductId);

        var totalItems = await reviewQuery.CountAsync(cancellationToken);

        var reviews = await reviewQuery
            .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

        return new GetReviewByProductResult(reviews, totalItems);
    }
}