
namespace Catalog.API.Categorys.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, string Name)
    : ICommand<UpdateCategoryResult>;
public record UpdateCategoryResult(bool IsSuccess);

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Category ID is required");

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 150).WithMessage("Name must be between 2 and 150 characters");
    }
}

internal class UpdateCategoryCommandHandler
    (IDocumentSession session)
    : ICommandHandler<UpdateCategoryCommand, UpdateCategoryResult>
{
    public async Task<UpdateCategoryResult> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await session.LoadAsync<Category>(command.Id, cancellationToken);

        if (category is null)
        {
            throw new CategoryNotFoundException(command.Id);
        }

        category.Name = command.Name;
        category.Modified = DateTime.UtcNow;

        session.Update(category);
        await session.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryResult(true);
    }
}
