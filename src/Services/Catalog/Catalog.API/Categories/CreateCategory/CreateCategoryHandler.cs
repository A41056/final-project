namespace Catalog.API.Categories.CreateCategory;

public record CreateCategoryCommand(List<string> Names, bool IsActive) : ICommand<CreateCategoryResult>;
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
        var existingCategories = await _session.Query<Category>()
            .Where(c => command.Names.Contains(c.Name))
            .ToListAsync(cancellationToken);

        var existingNames = existingCategories.Select(c => c.Name).ToHashSet();
        var duplicates = command.Names.Where(name => existingNames.Contains(name)).ToList();
        var namesToAdd = command.Names.Where(name => !existingNames.Contains(name)).Distinct().ToList();

        var createdIds = new List<Guid>();

        foreach (var name in namesToAdd)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
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