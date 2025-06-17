namespace Catalog.API.Categories.GetCategoryTree;

public record CategoryDto(Guid Id, string Name, string? Slug, List<CategoryDto> Subcategories);
public record GetCategoryTreeQuery() : IQuery<List<CategoryDto>>;

public class GetCategoryTreeHandler(IDocumentSession session)
    : IQueryHandler<GetCategoryTreeQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoryTreeQuery query, CancellationToken cancellationToken)
    {
        var categories = await session.Query<Category>()
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        var categoryLookup = categories.ToLookup(c => c.ParentId);

        List<CategoryDto> BuildTree(Guid? parentId)
        {
            return categoryLookup[parentId]
                .Select(c => new CategoryDto(
                    c.Id,
                    c.Name,
                    c.Slug,
                    BuildTree(c.Id)
                ))
                .ToList();
        }

        return BuildTree(null);
    }
}

