namespace Review.API.Reviews.DeleteReview;

public record DeleteReviewCommand(Guid Id) : ICommand<DeleteReviewResult>;
public record DeleteReviewResult(bool IsSuccess);

public class DeleteReviewCommandValidator : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Review ID is required");
    }
}

public class DeleteReviewCommandHandler
    (IDocumentSession session)
    : ICommandHandler<DeleteReviewCommand, DeleteReviewResult>
{
    public async Task<DeleteReviewResult> Handle(DeleteReviewCommand command, CancellationToken cancellationToken)
    {
        session.Delete<Models.Review>(command.Id);
        await session.SaveChangesAsync(cancellationToken);

        return new DeleteReviewResult(true);
    }
}
