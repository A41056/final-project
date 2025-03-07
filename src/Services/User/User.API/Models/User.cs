namespace User.API.Models;
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
    public int Age { get; set; }
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public int LoginFailedCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public Guid RoleId { get; set; }
}