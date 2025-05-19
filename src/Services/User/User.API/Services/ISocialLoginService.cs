namespace User.API.Services;

public interface ISocialLoginService
{
    Task<(bool IsValid, string Email, string FirstName, string LastName)> VerifyTokenAsync(string accessToken);
}
