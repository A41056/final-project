using BuildingBlocks.Exceptions;

namespace Media.API.Exceptions;

public class FileNotFoundException : NotFoundException
{
    public FileNotFoundException(Guid Id) : base("File", Id)
    {
    }
}
