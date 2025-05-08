namespace Catalog.API.Categories.GetCategoryPath;

using Catalog.API.Models;
using Marten;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public record GetCategoryPathQuery(Guid Id) : IQuery<GetCategoryPathResult>;
public record GetCategoryPathResult(List<Category> Path);

public class GetCategoryPathHandler : IQueryHandler<GetCategoryPathQuery, GetCategoryPathResult>
{
    private readonly IDocumentSession _session;

    public GetCategoryPathHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<GetCategoryPathResult> Handle(GetCategoryPathQuery query, CancellationToken cancellationToken)
    {
        var path = new List<Category>();
        var currentId = query.Id;
        var seenIds = new HashSet<Guid>();

        while (currentId != Guid.Empty)
        {
            if (!seenIds.Add(currentId))
            {
                throw new InvalidOperationException("Detected a cycle in category hierarchy.");
            }

            var category = await _session.LoadAsync<Category>(currentId, cancellationToken);
            if (category == null)
            {
                throw new CategoryNotFoundException(query.Id);
            }

            path.Insert(0, category);
            currentId = category.ParentId ?? Guid.Empty;
        }

        if (path.Count == 0)
        {
            throw new CategoryNotFoundException(query.Id);
        }

        return new GetCategoryPathResult(path);
    }
}

public class CategoryNotFoundException : Exception
{
    public CategoryNotFoundException(Guid id)
        : base($"Category with ID {id} not found.")
    {
    }
}