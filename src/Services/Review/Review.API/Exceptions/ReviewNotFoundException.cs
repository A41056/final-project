using BuildingBlocks.Exceptions;

namespace Review.API.Exceptions;

public class ReviewNotFoundException : NotFoundException
{
    public ReviewNotFoundException(Guid Id) : base("Review", Id)
    {
    }
}
