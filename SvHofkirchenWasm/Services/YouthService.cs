using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SvHofkirchenWasm.Models;

namespace SvHofkirchenWasm.Services;

public class YouthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js; // Neu: Für den Datei-Download (JS Interop)
    
    public List<MemberDto> YouthMembers { get; private set; } = new();
    public List<PresenceDto> Presences { get; private set; } = new();
    public bool IsInitialized { get; private set; }

    public event Action? OnChange;

    // Wir injizieren jetzt auch die JSRuntime für den Download
    public YouthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        try
        {
            // GET Request an Cloudflare Worker
            var root = await _http.GetFromJsonAsync<YouthDataRoot>("api/youth");
            
            if (root != null)
            {
                YouthMembers = root.Members ?? new();
                Presences = root.Presences ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der API-Daten: {ex.Message}");
        }

        IsInitialized = true;
        NotifyStateChanged();
    }

    // --- CRUD ---
    public async Task AddMemberAsync(MemberDto member)
    {
        int newId = YouthMembers.Any() ? YouthMembers.Max(m => m.MemberId) + 1 : 1;
        member.MemberId = newId;
        member.YouthStatus = 1;
        
        YouthMembers.Add(member);
        await SaveDataAsync();
    }

    public async Task UpdateMemberAsync(MemberDto updatedMember)
    {
        var existing = YouthMembers.FirstOrDefault(m => m.MemberId == updatedMember.MemberId);
        if (existing != null)
        {
            existing.FirstName = updatedMember.FirstName;
            existing.LastName = updatedMember.LastName;
            existing.BirthDateString = updatedMember.BirthDateString;
            await SaveDataAsync();
        }
    }

    public async Task DeleteMemberAsync(int memberId)
    {
        var member = YouthMembers.FirstOrDefault(m => m.MemberId == memberId);
        if (member != null)
        {
            YouthMembers.Remove(member);
            Presences.RemoveAll(p => p.MemberId == memberId);
            await SaveDataAsync();
        }
    }

    // --- ANWESENHEIT ---
    public async Task TogglePresenceAsync(int memberId, DateTime date)
    {
        var existing = Presences.FirstOrDefault(p => p.MemberId == memberId && p.ParsedDate.Date == date.Date);
        if (existing != null) Presences.Remove(existing);
        else Presences.Add(new PresenceDto { MemberId = memberId, ParsedDate = date });
        
        await SaveDataAsync();
    }

    public bool IsPresent(int memberId, DateTime date)
    {
        return Presences.Any(p => p.MemberId == memberId && p.ParsedDate.Date == date.Date);
    }

    // --- API & DATEI HANDLING ---

    // Speichert den aktuellen Stand in der Cloud
    private async Task SaveDataAsync()
    {
        try
        {
            var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
            
            // POST Request an Cloudflare Worker
            var response = await _http.PostAsJsonAsync("api/youth", root);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Fehler beim Speichern: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Fehler beim Speichern: {ex.Message}");
        }
        
        NotifyStateChanged();
    }

    // Exportiert die aktuellen API-Daten als JSON-Datei (Download im Browser)
    public async Task DownloadDataAsync()
    {
        try 
        {
            var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
            
            // JSON erzeugen
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(root, jsonOptions);
            
            // In Base64 umwandeln für den JS-Download
            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);
            
            // JS-Funktion in index.html aufrufen
            await _js.InvokeVoidAsync("downloadFileFromStream", "vereinsdaten_backup.json", base64);
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Export Fehler: {ex.Message}");
        }
    }

    // Importiert eine JSON-Datei, überschreibt lokal UND lädt in die Cloud hoch
    public async Task ImportDataAsync(IBrowserFile file)
    {
        try
        {
            // Datei lesen (Limit auf 5MB erhöht)
            using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 5); 
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            
            var root = JsonSerializer.Deserialize<YouthDataRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (root != null)
            {
                // Lokale Daten aktualisieren
                YouthMembers = root.Members ?? new();
                Presences = root.Presences ?? new();
                
                // SOFORT in die Cloud speichern
                await SaveDataAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Import Fehler: {ex.Message}");
        }
        
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}