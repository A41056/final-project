namespace User.API.User.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<GetUserByIdResult>;
public record GetUserByIdResult(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Phone,
    List<string> Address,
    string Gender,
    int Age,
    Guid RoleId,
    DateTime CreatedDate,
    DateTime? ModifiedDate
);

internal class GetUserByIdHandler(IDocumentSession session)
    : IRequestHandler<GetUserByIdQuery, GetUserByIdResult>
{
    public async Task<GetUserByIdResult> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await session.LoadAsync<Models.User>(query.Id);
        if (user == null || !user.IsActive) throw new KeyNotFoundException("User not found");

        return new GetUserByIdResult(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Phone,
            user.Address,
            user.Gender,
            user.Age,
            user.RoleId,
            user.CreatedDate,
            user.ModifiedDate
        );
    }
}
