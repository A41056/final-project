namespace Catalog.API.Categories.UpdateCategory;

using Catalog.API.Extensions;
using Catalog.API.Models;
using FluentValidation;
using Marten;
using System;
using System.Threading;
using System.Threading.Tasks;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Slug,
    Guid? ParentId,
    bool IsActive
) : ICommand<UpdateCategoryResult>;

public record UpdateCategoryResult(bool IsSuccess);

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Category ID is required");

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 150).WithMessage("Name must be between 2 and 150 characters");

        RuleFor(command => command.Slug)
            .Length(2, 150).When(command => !string.IsNullOrEmpty(command.Slug))
            .WithMessage("Slug must be between 2 and 150 characters")
            .Matches("^[a-z0-9-]+$").When(command => !string.IsNullOrEmpty(command.Slug))
            .WithMessage("Slug can only contain lowercase letters, numbers, and hyphens");

        RuleFor(command => command.ParentId)
            .Must(parentId => parentId != Guid.Empty).When(command => command.ParentId.HasValue)
            .WithMessage("ParentId must be a valid GUID");
    }
}

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, UpdateCategoryResult>
{
    private readonly IDocumentSession _session;

    public UpdateCategoryCommandHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<UpdateCategoryResult> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _session.LoadAsync<Category>(command.Id, cancellationToken);
        if (category == null)
        {
            throw new CategoryNotFoundException(command.Id);
        }

        if (command.ParentId.HasValue)
        {
            var parent = await _session.LoadAsync<Category>(command.ParentId.Value, cancellationToken);
            if (parent == null)
            {
                throw new CategoryNotFoundException(command.ParentId.Value);
            }
            if (command.ParentId == command.Id)
            {
                throw new InvalidOperationException("Category cannot be its own parent.");
            }
        }

        var generatedSlug = command.Slug ?? StringExtensions.GenerateSlug(command.Name);
        var existingCategories = await _session.Query<Category>()
            .Where(c => c.Id != command.Id &&
                       (c.Name == command.Name || c.Slug == generatedSlug))
            .ToListAsync(cancellationToken);

        if (existingCategories.Any(c => c.Name == command.Name))
        {
            throw new InvalidOperationException($"Category with name '{command.Name}' already exists.");
        }
        if (existingCategories.Any(c => c.Slug == generatedSlug))
        {
            throw new InvalidOperationException($"Category with slug '{generatedSlug}' already exists.");
        }

        category.Name = command.Name;
        category.Slug = generatedSlug;
        category.ParentId = command.ParentId;
        category.IsActive = command.IsActive;
        category.Modified = DateTime.UtcNow;

        _session.Update(category);
        await _session.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryResult(true);
    }
}

public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException(Guid id)
        : base($"Category with ID {id} not found.")
    {
    }
}