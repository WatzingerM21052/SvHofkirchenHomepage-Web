namespace SvHofkirchenHomepage.Models;

public class DatabaseRoot {
    public List<CategoryDto> Category { get; set; } = new();
    public List<ReportDto> Report { get; set; } = new();
    public List<AppointmentDto> Appointment { get; set; } = new();
    public List<MemberDto> Member { get; set; } = new();
    public List<YouthPresenceDto> YouthPresence { get; set; } = new();
    public List<WebaccessDto> Webaccess { get; set; } = new();
    public List<ImageDto> Image { get; set; } = new();
    public List<ReportImageDto> ReportImage { get; set; } = new();
    public List<UserDto> User { get; set; } = new();
    public List<TokenDto> Token { get; set; } = new();
}

public class CategoryDto {
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryShort { get; set; } = "";
}

public class ReportDto {
    public int ReportId { get; set; }
    public string ReportTitle { get; set; } = "";
    public string ReportText { get; set; } = "";
    public string ReportAboutDate { get; set; } = "";
    public string ReportWrittenDate { get; set; } = "";
    public int CategoryId { get; set; }
    public List<string> ImagePaths { get; set; } = new();
}

public class AppointmentDto {
    public int AppointmentId { get; set; }
    public string AppointmentTitle { get; set; } = "";
    public string AppointmentLocation { get; set; } = "";
    public string AppointmentDate { get; set; } = "";
    // FIX: Dieses Feld hat gefehlt
    public string? AppointmentTime { get; set; } 
    public string? AppointmentDescription { get; set; }
    public int CategoryId { get; set; }
}

public class MemberDto {
    public int MemberId { get; set; }
    public string MemberFirstName { get; set; } = "";
    public string MemberLastName { get; set; } = "";
    // FIX: Diese Felder haben gefehlt
    public string MemberBirthdate { get; set; } = "";
    public int? MemberElo { get; set; }
    public int YouthStatus { get; set; } // 1 = Jugend, 0 = Erwachsen
}

public class YouthPresenceDto {
    public int PresenceId { get; set; }
    public int MemberId { get; set; }
    public string PresenceDate { get; set; } = "";
}

public class WebaccessDto {
    public int WebaccessId { get; set; }
    public string Date { get; set; } = "";
}

public class ImageDto {
    public int ImageId { get; set; }
    public string ImagePath { get; set; } = "";
    public string ImageUploadedDate { get; set; } = "";
}

public class ReportImageDto {
    public int ReportImageId { get; set; }
    public int ReportId { get; set; }
    public int ImageId { get; set; }
}

public class UserDto {
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public string UserPasswordHash { get; set; } = "";
    public int PermissionLevel { get; set; }
    public string? UserEmail { get; set; }
    public int? MemberId { get; set; }
}

public class TokenDto {
    public int TokenId { get; set; }
    public string TokenString { get; set; } = "";
    public string ExpiryDate { get; set; } = "";
    public int UserId { get; set; }
}