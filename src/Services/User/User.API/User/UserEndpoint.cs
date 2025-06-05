using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using User.API.Common;
using User.API.Dtos;
using User.API.Helpers;
using User.API.Models;
using User.API.Services;

namespace User.API.User;

public record RegisterDto(string Username, string Email, string FirstName, string LastName, string Password, string Phone, List<string> Address, string Gender, int Age, Guid RoleId);
public record LoginDto(string Email, string Password);
public record UpdateUserDto(string? Username, string? Email, string? FirstName, string? LastName, string? Phone, List<string>? Address, string? Gender, int? Age);

public class UserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Đăng ký người dùng
        app.MapPost("/register", async (RegisterDto dto, IDocumentSession session) =>
        {
            var existingUser = await session.Query<Models.User>().FirstOrDefaultAsync(u => u.Email == dto.Email || u.Username == dto.Username);
            if (existingUser != null) return Results.BadRequest("User already exists");

            var (hash, salt) = HashHelper.HashPassword(dto.Password);
            var user = new Models.User
            {
                Username = dto.Username,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Address = dto.Address,
                Gender = dto.Gender,
                Age = dto.Age,
                PasswordHash = hash,
                PasswordSalt = salt,
                RoleId = dto.RoleId
            };

            session.Store(user);
            await session.SaveChangesAsync();
            return Results.Ok("User registered successfully");
        }).AllowAnonymous();

        // Đăng nhập
        app.MapPost("/login", async (LoginDto dto, IDocumentSession session, IConfiguration config) =>
        {
            var user = await session.Query<Models.User>().FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);
            if (user == null || !HashHelper.VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
            {
                if (user != null)
                {
                    user.LoginFailedCount++;
                    session.Store(user);
                    await session.SaveChangesAsync();
                }
                return Results.Unauthorized();
            }

            user.LoginFailedCount = 0;
            user.RefreshToken = Guid.NewGuid().ToString();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            session.Store(user);
            await session.SaveChangesAsync();

            var token = GenerateJwtToken(user, config);
            var userDto = new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender,
                Age = user.Age,
                RoleId = user.RoleId,
                CreatedDate = user.CreatedDate,
                ModifiedDate = user.ModifiedDate
            };

            return Results.Ok(new
            {
                Token = token,
                RefreshToken = user.RefreshToken,
                User = userDto
            });
        }).AllowAnonymous();

        app.MapPost("/refresh-token", async (RefreshTokenDto dto, IDocumentSession session, IConfiguration config) =>
        {
            var user = await session.Query<Models.User>()
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Results.Unauthorized();

            var newToken = GenerateJwtToken(user, config);
            var newRefreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            session.Store(user);
            await session.SaveChangesAsync();

            return Results.Ok(new
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            });
        }).AllowAnonymous();

        app.MapPost("/external-login", async (
    ExternalLoginDto dto,
    SocialLoginFactory loginFactory,
    IDocumentSession session,
    IConfiguration config) =>
        {
            var service = loginFactory.GetService(dto.Provider);
            var (isValid, email, firstName, lastName) = await service.VerifyTokenAsync(dto.AccessToken);

            if (!isValid || string.IsNullOrEmpty(email))
                return Results.Unauthorized();

            var user = await session.Query<Models.User>().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new Models.User
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Username = email.Split('@')[0],
                    IsActive = true,
                    RoleId = Guid.Parse("user-role-id")
                };

                session.Store(user);
                await session.SaveChangesAsync();
            }

            var token = GenerateJwtToken(user, config);
            var userDto = new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                RoleId = user.RoleId
            };

            return Results.Ok(new { Token = token, User = userDto });
        }).AllowAnonymous();

        app.MapPost("/reset-password", async (
    ResetPasswordRequest request,
    IDocumentSession session,
    IEmailService emailService,
    CancellationToken ct) =>
        {
            var user = await session.Query<Models.User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, ct);

            if (user is null)
                return Results.NotFound("User not found");

            // Tạo mật khẩu mới
            string newPassword = GenerateRandomPassword(10);

            // Băm lại
            var (hash, salt) = HashHelper.HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            session.Store(user);
            await session.SaveChangesAsync(ct);

            // Soạn email
            var body = $"""
        <p>Xin chào {user.FirstName + " " + user.LastName},</p>
        <p>Bạn đã yêu cầu đặt lại mật khẩu. Dưới đây là mật khẩu mới của bạn:</p>
        <p><strong>{newPassword}</strong></p>
        <p>Vui lòng đăng nhập và thay đổi mật khẩu ngay sau đó.</p>
    """;

            await emailService.SendEmailAsync(new MailRequest
            {
                ToAddress = user.Email,
                Subject = "Mật khẩu mới của bạn",
                Body = body
            }, ct);

            return Results.Ok("Mật khẩu mới đã được gửi qua email");
        }).AllowAnonymous();

        // Thêm người dùng (yêu cầu quyền admin)
        app.MapPost("/users", async (RegisterDto dto, IDocumentSession session, HttpContext context) =>
        {
            var roleId = context.User.FindFirstValue("roleId");
            if (roleId != Constants.AdminRoleId) return Results.Forbid();

            var (hash, salt) = HashHelper.HashPassword(dto.Password);
            var user = new Models.User
            {
                Username = dto.Username,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Address = dto.Address,
                Gender = dto.Gender,
                Age = dto.Age,
                PasswordHash = hash,
                PasswordSalt = salt,
                RoleId = dto.RoleId
            };

            session.Store(user);
            await session.SaveChangesAsync();
            return Results.Ok("User added successfully");
        }).RequireAuthorization();

        // Sửa thông tin người dùng
        app.MapPut("/users/{id}", async (Guid id, UpdateUserDto dto, IDocumentSession session) =>
       {
           var user = await session.LoadAsync<Models.User>(id);
           if (user == null || !user.IsActive) return Results.NotFound();

           user.Username = dto.Username ?? user.Username;
           user.Email = dto.Email ?? user.Email;
           user.FirstName = dto.FirstName ?? user.FirstName;
           user.LastName = dto.LastName ?? user.LastName;
           user.Phone = dto.Phone ?? user.Phone;
           user.Address = dto.Address ?? user.Address;
           user.Gender = dto.Gender ?? user.Gender;
           user.Age = dto.Age ?? user.Age;
           user.ModifiedDate = DateTime.UtcNow;

           session.Store(user);
           await session.SaveChangesAsync();
           return Results.Ok("User updated successfully");
       }).RequireAuthorization();

        // Xóa (de-active) người dùng
        app.MapDelete("/users/{id}", async (Guid id, IDocumentSession session) =>
        {
            var user = await session.LoadAsync<Models.User>(id);
            if (user == null) return Results.NotFound();

            user.IsActive = false;
            user.ModifiedDate = DateTime.UtcNow;
            session.Store(user);
            await session.SaveChangesAsync();
            return Results.Ok("User deactivated successfully");
        }).RequireAuthorization();

        // Lấy thông tin người dùng theo ID
        app.MapGet("/users/{id}", async (Guid id, IDocumentSession session) =>
        {
            var user = await session.LoadAsync<Models.User>(id);
            if (user == null || !user.IsActive) return Results.NotFound();

            // Trả về DTO để không lộ thông tin nhạy cảm như PasswordHash, PasswordSalt
            var userDto = new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender,
                Age = user.Age,
                RoleId = user.RoleId,
                CreatedDate = user.CreatedDate,
                ModifiedDate = user.ModifiedDate
            };

            return Results.Ok(userDto);
        }).RequireAuthorization();

        // Lấy danh sách người dùng
        app.MapGet("/users", async (IDocumentSession session, HttpContext context) =>
        {
            var roleId = context.User.FindFirstValue("roleId");
            if (roleId != Constants.AdminRoleId) return Results.Forbid();

            var users = await session.Query<Models.User>()
                                    .Where(u => u.IsActive)
                                    .ToListAsync();

            var userDtos = users.Select(user => new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender,
                Age = user.Age,
                RoleId = user.RoleId,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                ModifiedDate = user.ModifiedDate
            });

            return Results.Ok(userDtos);
        }).RequireAuthorization();
    }

    private string GenerateJwtToken(Models.User user, IConfiguration config)
    {
        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("roleId", user.RoleId.ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRandomPassword(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
