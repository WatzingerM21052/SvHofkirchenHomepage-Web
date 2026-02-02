namespace SvHofkirchenWasm.Models;

public class User
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Admin, Trainer, Mitglied, Besucher
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLogin { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
    public string? Token { get; set; }
}
