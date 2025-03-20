using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using User.API.Helpers;

namespace User.API.User;

public record RegisterDto(string Username, string Email, string Password, string Phone, string Address, string Gender, int Age, Guid RoleId);
public record LoginDto(string Email, string Password);
public record UpdateUserDto(string? Username, string? Email, string? Phone, string? Address, string? Gender, int? Age);

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
            session.Store(user);
            await session.SaveChangesAsync();

            var token = GenerateJwtToken(user, config);
            var userDto = new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender,
                Age = user.Age,
                RoleId = user.RoleId,
                CreatedDate = user.CreatedDate,
                ModifiedDate = user.ModifiedDate
            };

            return Results.Ok(new { Token = token, User = userDto });
        }).AllowAnonymous();

        // Thêm người dùng (yêu cầu quyền admin)
        app.MapPost("/users", async (RegisterDto dto, IDocumentSession session, HttpContext context) =>
        {
            var roleId = context.User.FindFirstValue("roleId");
            if (roleId != "admin-role-id") return Results.Forbid(); // Giả sử admin-role-id là RoleId của admin

            var (hash, salt) = HashHelper.HashPassword(dto.Password);
            var user = new Models.User
            {
                Username = dto.Username,
                Email = dto.Email,
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
            // Kiểm tra quyền admin (tùy chọn)
            var roleId = context.User.FindFirstValue("roleId");
            if (roleId != "admin-role-id") return Results.Forbid(); // Giả sử "admin-role-id" là RoleId của admin

            var users = await session.Query<Models.User>()
                                    .Where(u => u.IsActive)
                                    .ToListAsync();

            // Chuyển thành DTO để trả về
            var userDtos = users.Select(user => new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Gender = user.Gender,
                Age = user.Age,
                RoleId = user.RoleId,
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
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("roleId", user.RoleId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
