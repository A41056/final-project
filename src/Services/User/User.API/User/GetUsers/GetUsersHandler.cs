namespace User.API.User.GetUsers;
public record GetUsersQuery(string RoleId) : IRequest<List<UserDto>>;
public record UserDto(
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
    bool IsActive,
    DateTime CreatedDate,
    DateTime? ModifiedDate
);
internal class GetUsersHandler(IDocumentSession session)
    : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await session.Query<Models.User>()
            .Where(u => u.IsActive)
            .ToListAsync(cancellationToken);

        return users.Select(user => new UserDto(
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
            user.IsActive,
            user.CreatedDate,
            user.ModifiedDate
        )).ToList();
    }
}
