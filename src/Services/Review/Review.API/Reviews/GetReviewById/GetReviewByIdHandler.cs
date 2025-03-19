namespace Review.API.Reviews.GetReviewById;

public record GetReviewByIdQuery(Guid Id) : IQuery<GetReviewByIdResult>;
public record GetReviewByIdResult(Models.Review Review);

internal class GetReviewByIdQueryHandler
    (IDocumentSession session)
    : IQueryHandler<GetReviewByIdQuery, GetReviewByIdResult>
{
    public async Task<GetReviewByIdResult> Handle(GetReviewByIdQuery query, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<Models.Review>(query.Id, cancellationToken);

        if (product is null)
        {
            throw new ReviewNotFoundException(query.Id);
        }

        return new GetReviewByIdResult(product);
    }
}
