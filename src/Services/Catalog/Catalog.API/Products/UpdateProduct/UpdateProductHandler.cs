using Catalog.API.Products.CreateProduct;

namespace Catalog.API.Products.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, List<Guid> Category, string Description, List<string> ImageFiles, bool IsHot, bool IsActive, List<ProductVariant> Variants)
    : ICommand<UpdateProductResult>;
public record UpdateProductResult(bool IsSuccess);

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Product ID is required");
        RuleFor(command => command.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(command => command.Variants).NotEmpty().WithMessage("At least one variant is required");
        RuleForEach(command => command.Variants).SetValidator(new ProductVariantValidator());
    }
}

internal class UpdateProductCommandHandler
    (IDocumentSession session)
    : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await session.LoadAsync<Product>(command.Id, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(command.Id);
        }

        product.Name = command.Name;
        product.CategoryIds = command.Category;
        product.Description = command.Description;
        product.ImageFiles = command.ImageFiles;
        product.IsHot = command.IsHot;
        product.IsActive = command.IsActive;
        product.Variants = command.Variants;
        product.Modified = DateTime.UtcNow;

        session.Update(product);
        await session.SaveChangesAsync(cancellationToken);

        return new UpdateProductResult(true);
    }
}