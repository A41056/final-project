namespace Review.API.Reviews.GetReviews;

public record GetReviewsQuery(int? PageNumber = 1, int? PageSize = 10) : IQuery<GetReviewsResult>;
public record GetReviewsResult(IEnumerable<Models.Review> Reviews);

public class GetReviewsQueryHandler
    (IDocumentSession session)
    : IQueryHandler<GetReviewsQuery, GetReviewsResult>
{
    public async Task<GetReviewsResult> Handle(GetReviewsQuery query, CancellationToken cancellationToken)
    {
        var products = await session.Query<Models.Review>()
            .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);

        return new GetReviewsResult(products);
    }
}
