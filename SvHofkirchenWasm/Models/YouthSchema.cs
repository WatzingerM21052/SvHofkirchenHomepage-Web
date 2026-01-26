using System.Text.Json.Serialization;

namespace SvHofkirchenWasm.Models;

// Container für das Laden der alten database.json
public class LegacyDatabase
{
    public List<MemberDto> Member { get; set; } = new();
    public List<PresenceDto> YouthPresence { get; set; } = new();
}

// Unser sauberes neues Format (youth.json)
public class YouthDataRoot
{
    public List<MemberDto> Members { get; set; } = new();
    public List<PresenceDto> Presences { get; set; } = new();
}

public class MemberDto
{
    public int MemberId { get; set; }
    [JsonPropertyName("MemberFirstName")] // Mappt auf alten Namen
    public string FirstName { get; set; } = "";
    [JsonPropertyName("MemberLastName")] // Mappt auf alten Namen
    public string LastName { get; set; } = "";
    [JsonPropertyName("MemberBirthdate")] 
    public string? BirthDateString { get; set; } // Format dd-MM-yyyy in DB
    
    // 1 = Jugend, 0/null = Erwachsene
    public int? YouthStatus { get; set; }

    // Helper für Anzeige
    public string FullName => $"{FirstName} {LastName}";
    
    // Berechnet das Alter (optional)
    public int Age 
    {
        get 
        {
            if (DateTime.TryParseExact(BirthDateString, new[] {"dd-MM-yyyy", "dd.MM.yyyy", "yyyy-MM-dd"}, null, System.Globalization.DateTimeStyles.None, out var date))
            {
                var today = DateTime.Today;
                var age = today.Year - date.Year;
                if (date.Date > today.AddYears(-age)) age--;
                return age;
            }
            return 0;
        }
    }
}

public class PresenceDto
{
    public int PresenceId { get; set; } // Optional für neue Einträge
    public int MemberId { get; set; }
    
    [JsonPropertyName("PresenceDate")]
    public string DateString { get; set; } = ""; // Format dd.MM.yyyy oder ISO

    [JsonIgnore]
    public DateTime ParsedDate 
    {
        get 
        {
            if (DateTime.TryParseExact(DateString, new[] {"dd.MM.yyyy", "yyyy-MM-dd", "dd-MM-yyyy"}, null, System.Globalization.DateTimeStyles.None, out var date))
                return date;
            return DateTime.MinValue;
        }
        set => DateString = value.ToString("yyyy-MM-dd");
    }
}