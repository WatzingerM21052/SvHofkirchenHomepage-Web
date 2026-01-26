// Models/AppSchema.cs
using System.Text.Json.Serialization;

namespace SvHofkirchenWasm.Models;

// Das Root-Objekt repräsentiert deine komplette database.json
public class DatabaseRoot
{
    public List<UserDto> User { get; set; } = new();
    public List<MemberDto> Members { get; set; } = new();
    public List<ReportDto> Reports { get; set; } = new();
    public List<AppointmentDto> Appointments { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}

public class UserDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int PermissionLevel { get; set; }
}

public class MemberDto
{
    public int MemberId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Function { get; set; }
    public string? ImagePath { get; set; } // Pfad zum Bild im wwwroot/Pictures Ordner
    public bool IsBoardMember { get; set; }
}

public class ReportDto
{
    public int ReportId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // HTML oder Text
    public DateTime Date { get; set; }
    public string? ImageUrl { get; set; }
}

public class AppointmentDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}