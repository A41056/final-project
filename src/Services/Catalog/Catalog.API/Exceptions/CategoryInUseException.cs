namespace Catalog.API.Exceptions;

public class CategoryInUseException : Exception
{
    public CategoryInUseException(string message) : base(message)
    {
    }
}
