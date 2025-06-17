using FluentValidation;
using User.API.Helpers;

namespace User.API.User.RefreshToken;
public record RefreshTokenRequest(string RefreshToken);

public record RefreshTokenResponse(string Token, string RefreshToken);
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
internal class RefreshTokenHandler(IDocumentSession session, IConfiguration config)
    : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand cmd, CancellationToken cancellationToken)
    {
        var user = await session.Query<Models.User>().FirstOrDefaultAsync(u => u.RefreshToken == cmd.RefreshToken);
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");

        var newToken = JwtHelper.GenerateJwtToken(user, config);
        var newRefreshToken = Guid.NewGuid().ToString();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        session.Store(user);
        await session.SaveChangesAsync();

        return new RefreshTokenResult(newToken, newRefreshToken);
    }
}
