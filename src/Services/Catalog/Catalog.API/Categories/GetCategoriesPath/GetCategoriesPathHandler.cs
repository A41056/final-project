namespace Catalog.API.Categories.GetCategoriesPath
{
    public record CategoryWithPath
    {
        public Category Category { get; init; }
        public List<string> Path { get; init; }
    }

    public class GetCategoriesWithPathHandler : IRequestHandler<GetCategoriesWithPathQuery, List<CategoryWithPath>>
    {
        private readonly IDocumentSession _session;

        public GetCategoriesWithPathHandler(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<List<CategoryWithPath>> Handle(GetCategoriesWithPathQuery request, CancellationToken cancellationToken)
        {
            var categories = await _session.Query<Category>().ToListAsync(cancellationToken);

            var categoryDict = categories.ToDictionary(c => c.Id);

            var result = new List<CategoryWithPath>();

            foreach (var cat in categories)
            {
                var pathNames = new List<string>();
                var currentId = cat.Id;
                var visited = new HashSet<Guid>();

                while (currentId != Guid.Empty && categoryDict.TryGetValue(currentId, out var currentCat))
                {
                    if (!visited.Add(currentId))
                    {
                        throw new InvalidOperationException("Cycle detected in categories");
                    }
                    pathNames.Insert(0, currentCat.Name);
                    currentId = currentCat.ParentId ?? Guid.Empty;
                }

                result.Add(new CategoryWithPath
                {
                    Category = cat,
                    Path = pathNames
                });
            }

            return result;
        }
    }

    public record GetCategoriesWithPathQuery : IRequest<List<CategoryWithPath>>;
}
