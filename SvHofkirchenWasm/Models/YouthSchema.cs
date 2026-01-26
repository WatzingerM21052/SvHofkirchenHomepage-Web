using System.Text.Json.Serialization;

namespace SvHofkirchenWasm.Models;

public class LegacyDatabase
{
    public List<MemberDto> Member { get; set; } = new();
    public List<PresenceDto> YouthPresence { get; set; } = new();
}

public class YouthDataRoot
{
    public List<MemberDto> Members { get; set; } = new();
    public List<PresenceDto> Presences { get; set; } = new();
}

public class MemberDto
{
    public int MemberId { get; set; }
    
    [JsonPropertyName("MemberFirstName")]
    public string FirstName { get; set; } = "";
    
    [JsonPropertyName("MemberLastName")]
    public string LastName { get; set; } = "";
    
    [JsonPropertyName("MemberBirthdate")] 
    public string? BirthDateString { get; set; }
    
    public int? YouthStatus { get; set; }

    public string FullName => $"{LastName} {FirstName}";
    
    public DateTime? BirthDate 
    {
        get 
        {
            if (DateTime.TryParseExact(BirthDateString, new[] {"dd-MM-yyyy", "dd.MM.yyyy", "yyyy-MM-dd"}, null, System.Globalization.DateTimeStyles.None, out var date))
                return date;
            return null;
        }
    }

    // --- KORRIGIERTE ALTERSKLASSEN LOGIK ---
    public string AgeCategory 
    {
        get 
        {
            if (!BirthDate.HasValue) return "-";

            var now = DateTime.Now;
            
            // Wir bestimmen das "Saison-Startjahr".
            // Wenn wir ab September sind (z.B. Okt 2025), zählt das Jahr 2025.
            // Wenn wir im Frühling sind (z.B. Jan 2026), gehören wir immer noch zur Saison, die 2025 startete.
            int seasonStartYear = (now.Month >= 9) ? now.Year : now.Year - 1;

            // Das "Turnieralter" berechnet sich aus Saisonstartjahr - Geburtsjahr.
            // Beispiel 2017er Kind in Saison 25/26: 2025 - 2017 = 8 -> U8.
            int ageInSeason = seasonStartYear - BirthDate.Value.Year;

            if (ageInSeason <= 8) return "U8";
            if (ageInSeason <= 10) return "U10";
            if (ageInSeason <= 12) return "U12";
            if (ageInSeason <= 14) return "U14";
            if (ageInSeason <= 16) return "U16";
            if (ageInSeason <= 18) return "U18";
            
            return "U20+"; // Erwachsen/U20
        }
    }

    public int Age 
    {
        get 
        {
            if (BirthDate.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Value.Year;
                if (BirthDate.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
            return 0;
        }
    }

    public string CategoryColor 
    {
        get 
        {
            return AgeCategory switch 
            {
                "U8" => "bg-success text-white",      // Grün
                "U10" => "bg-info text-dark",         // Türkis
                "U12" => "bg-primary text-white",     // Blau
                "U14" => "bg-warning text-dark",      // Gelb
                "U16" => "bg-danger text-white",      // Rot
                "U18" => "bg-dark text-white",        // Schwarz
                _ => "bg-secondary text-white"        // Grau
            };
        }
    }
}

public class PresenceDto
{
    public int PresenceId { get; set; }
    public int MemberId { get; set; }
    
    [JsonPropertyName("PresenceDate")]
    public string DateString { get; set; } = "";

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