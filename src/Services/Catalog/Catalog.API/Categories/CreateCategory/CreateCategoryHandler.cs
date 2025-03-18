namespace Catalog.API.Categories.CreateCategory;

public record CreateCategoryCommand(string Name) : ICommand<CreateCategoryResult>;
public record CreateCategoryResult(Guid Id);

public class CreateCategoryHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryResult>
{
    private readonly IDocumentSession _session;

    public CreateCategoryHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<CreateCategoryResult> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Created = DateTime.UtcNow,
            Modified = DateTime.UtcNow,
            IsActive = true
        };

        _session.Store(category);
        await _session.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResult(category.Id);
    }
}
