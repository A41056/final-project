namespace Catalog.API.Exceptions;

public class DuplicateCategoryException : Exception
{
    public DuplicateCategoryException(string message) : base(message)
    {
    }
}
