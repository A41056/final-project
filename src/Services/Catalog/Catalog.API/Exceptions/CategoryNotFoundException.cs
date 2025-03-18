using BuildingBlocks.Exceptions;

namespace Catalog.API.Exceptions;

public class CategoryNotFoundException : NotFoundException
{
    public CategoryNotFoundException(Guid Id) : base("Category", Id)
    {
    }
}
