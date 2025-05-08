namespace Catalog.API.Categories.CreateCategory;

using Catalog.API.Extensions;
using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public record CreateCategoryCommand(
    string Names,
    bool IsActive,
    Guid? ParentId = null
) : ICommand<CreateCategoryResult>;
public record CreateCategoryResult(List<Guid> CreatedIds, List<string> Duplicates);

public class CreateCategoryHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly IDocumentSession _session;

    public CreateCategoryHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<CreateCategoryResult> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var nameList = command.Names
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();

        if (!nameList.Any())
        {
            return new CreateCategoryResult(new List<Guid>(), new List<string>());
        }

        var slugList = nameList.Select(n => StringExtensions.GenerateSlug(n)).ToList();

        var existingCategories = await _session.Query<Category>()
            .Where(c => c.Name.In(nameList) || c.Slug.In(slugList))
            .ToListAsync(cancellationToken);

        var existingNames = existingCategories.Select(c => c.Name).ToHashSet();
        var existingSlugs = existingCategories.Select(c => c.Slug).ToHashSet();
        var duplicates = new List<string>();
        var categoriesToAdd = new List<(string Name, string Slug)>();

        foreach (var name in nameList)
        {
            var slug = StringExtensions.GenerateSlug(name);
            if (existingNames.Contains(name) || existingSlugs.Contains(slug))
            {
                duplicates.Add(name);
                continue;
            }
            categoriesToAdd.Add((name, slug));
        }

        var createdIds = new List<Guid>();

        foreach (var (name, slug) in categoriesToAdd)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = slug,
                ParentId = command.ParentId,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                IsActive = command.IsActive
            };
            _session.Store(category);
            createdIds.Add(category.Id);
        }

        if (createdIds.Any())
        {
            await _session.SaveChangesAsync(cancellationToken);
        }

        return new CreateCategoryResult(createdIds, duplicates);
    }
}