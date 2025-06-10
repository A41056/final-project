namespace Review.API.Reviews.UpdateReview;

public record UpdateReviewCommand(Guid Id, Guid ProductId, Guid UserId, int Rating, string Comment)
    : ICommand<UpdateReviewResult>;
public record UpdateReviewResult(bool IsSuccess);

public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Review ID is required");

        RuleFor(command => command.Comment)
            .NotEmpty().WithMessage("Comment is required")
            .Length(2, 150).WithMessage("Comment must be between 2 and 150 characters");

        RuleFor(command => command.Rating)
            .GreaterThan(0).WithMessage("Rating must be greater than 0");
    }
}

public class UpdateReviewCommandHandler
    (IDocumentSession session)
    : ICommandHandler<UpdateReviewCommand, UpdateReviewResult>
{
    public async Task<UpdateReviewResult> Handle(UpdateReviewCommand command, CancellationToken cancellationToken)
    {
        var review = await session.LoadAsync<Models.Review>(command.Id, cancellationToken);

        if (review is null)
        {
            throw new ReviewNotFoundException(command.Id);
        }

        review.Comment = command.Comment;
        review.Rating = command.Rating;
        review.Modified = DateTime.UtcNow;

        session.Update(review);
        await session.SaveChangesAsync(cancellationToken);

        return new UpdateReviewResult(true);
    }
}
