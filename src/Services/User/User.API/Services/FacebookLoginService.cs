using System.Text.Json;

namespace User.API.Services;

public class FacebookLoginService : ISocialLoginService
{
    public async Task<(bool, string, string, string)> VerifyTokenAsync(string token)
    {
        var client = new HttpClient();
        var response = await client.GetAsync($"https://graph.facebook.com/me?fields=id,email,first_name,last_name&access_token={token}");

        if (!response.IsSuccessStatusCode) return (false, null!, null!, null!);

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var email = payload.GetProperty("email").GetString();
        var firstName = payload.GetProperty("first_name").GetString();
        var lastName = payload.GetProperty("last_name").GetString();

        return (true, email!, firstName!, lastName!);
    }
}