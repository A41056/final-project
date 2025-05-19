namespace User.API.Services;

public class SocialLoginFactory
{
    private readonly IServiceProvider _provider;

    public SocialLoginFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ISocialLoginService GetService(string provider)
    {
        return provider.ToLower() switch
        {
            "google" => _provider.GetRequiredService<GoogleLoginService>(),
            "facebook" => _provider.GetRequiredService<FacebookLoginService>(),
            _ => throw new NotSupportedException("Unsupported login provider")
        };
    }
}
