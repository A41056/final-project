using Catalog.API.Extensions;
using Catalog.API.Services;

namespace Catalog.API.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    List<Guid> CategoryIds,
    string Description,
    List<string> ImageFiles,
    bool IsHot,
    bool IsActive,
    List<string> Tags,
    List<ProductVariant> Variants
) : ICommand<CreateProductResult>;

public record CreateProductResult(Guid Id);

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.CategoryIds).NotEmpty().WithMessage("Category is required");
        RuleFor(x => x.Variants).NotEmpty().WithMessage("At least one variant is required");
        RuleForEach(x => x.Variants).SetValidator(new ProductVariantValidator());
    }
}

public class ProductVariantValidator : AbstractValidator<ProductVariant>
{
    public ProductVariantValidator()
    {
        RuleFor(x => x.Properties)
            .NotEmpty().WithMessage("At least one property is required for variant")
            .Must(props => props.All(p => !string.IsNullOrEmpty(p.Type) && !string.IsNullOrEmpty(p.Value)))
            .WithMessage("Each property must have a non-empty Type and Value");

        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Variant price must be greater than 0");
        RuleFor(x => x.StockCount).GreaterThanOrEqualTo(0).WithMessage("Stock count must be non-negative");
    }
}

internal class CreateProductHandler(IDocumentSession session, IElasticSearchService esService)
    : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = command.Name,
            CategoryIds = command.CategoryIds,
            Description = command.Description,
            ImageFiles = command.ImageFiles,
            IsHot = command.IsHot,
            IsActive = command.IsActive,
            Variants = command.Variants,
            Tags = command.Tags.Select(x => x.NormalizeTag()).ToList(),
            AverageRating = 0.0,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow
        };

        session.Store(product);
        await session.SaveChangesAsync(cancellationToken);

        var esProduct = new ProductElasticModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageFiles = command.ImageFiles,
            CategoryIds = product.CategoryIds,
            IsHot = product.IsHot,
            IsActive = product.IsActive,
            Tags = command.Tags.Select(x => x.NormalizeTag()).ToList(),
            Variants = product.Variants.Select(v => v.ToElastic()).ToList(),
            Modified = DateTime.UtcNow,
        };
        await esService.UpdateProductAsync(esProduct, cancellationToken);

        return new CreateProductResult(product.Id);
    }
}