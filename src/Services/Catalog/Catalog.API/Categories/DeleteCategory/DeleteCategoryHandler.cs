
namespace Catalog.API.Categorys.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : ICommand<DeleteCategoryResult>;
public record DeleteCategoryResult(bool IsSuccess);

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Category ID is required");
    }
}

internal class DeleteCategoryCommandHandler(IDocumentSession session)
    : ICommandHandler<DeleteCategoryCommand, DeleteCategoryResult>
{
    public async Task<DeleteCategoryResult> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var productsUsingCategory = await session.Query<Product>()
            .Where(p => p.CategoryIds.Contains(command.Id))
            .AnyAsync(cancellationToken);

        if (productsUsingCategory)
        {
            throw new CategoryInUseException($"Cannot delete category with ID '{command.Id}' because it is being used by one or more products.");
        }

        session.Delete<Category>(command.Id);
        await session.SaveChangesAsync(cancellationToken);

        return new DeleteCategoryResult(true);
    }
}
