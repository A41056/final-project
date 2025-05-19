namespace User.API.Dtos;

public record ExternalLoginDto(string Provider, string AccessToken);