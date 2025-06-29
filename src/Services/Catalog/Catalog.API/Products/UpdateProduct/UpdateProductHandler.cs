﻿using Catalog.API.Extensions;
using Catalog.API.Products.CreateProduct;
using Catalog.API.Services;

namespace Catalog.API.Products.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, List<Guid> Category, string Description, List<string> ImageFiles, bool IsHot, bool IsActive, List<string> Tags, List<ProductVariant> Variants)
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
    (IDocumentSession session, IElasticSearchService esService)
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
        product.Tags = command.Tags.Select(x => x.NormalizeTag()).ToList();
        product.Variants = command.Variants;
        product.Modified = DateTime.UtcNow;

        session.Update(product);
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

        return new UpdateProductResult(true);
    }
}