using Marten;

namespace Review.API.Reviews.CreateReview;

public record CreateReviewCommand(Guid ProductId, Guid UserId, int Rating, string Comment)
    : ICommand<CreateReviewResult>;
public record CreateReviewResult(Guid Id);

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.Comment).NotEmpty().WithMessage("Comment is required");
        RuleFor(x => x.Rating).GreaterThan(0).WithMessage("Rating must be greater than 0");
    }
}

internal class CreateReviewCommandHandler
    (IDocumentSession session)
    : ICommandHandler<CreateReviewCommand, CreateReviewResult>
{
    public async Task<CreateReviewResult> Handle(CreateReviewCommand command, CancellationToken cancellationToken)
    {
        //create Review entity from command object
        //save to database
        //return CreateReviewResult result               

        var product = new Models.Review
        {
            Id = Guid.NewGuid(),
            ProductId = command.ProductId,
            UserId = command.UserId,
            Rating = command.Rating,
            Comment = command.Comment,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            IsActive = true
        };
        
        //save to database
        session.Store(product);
        await session.SaveChangesAsync(cancellationToken);

        //return result
        return new CreateReviewResult(product.Id);
    }
}
