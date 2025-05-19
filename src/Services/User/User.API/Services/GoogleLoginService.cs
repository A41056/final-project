using System.Text.Json;

namespace User.API.Services;

public class GoogleLoginService : ISocialLoginService
{
    public async Task<(bool, string, string, string)> VerifyTokenAsync(string token)
    {
        var client = new HttpClient();
        var response = await client.GetAsync($"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={token}");

        if (!response.IsSuccessStatusCode) return (false, null!, null!, null!);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var email = payload.GetProperty("email").GetString();
        var givenName = payload.GetProperty("given_name").GetString();
        var familyName = payload.GetProperty("family_name").GetString();

        return (true, email!, givenName!, familyName!);
    }
}
