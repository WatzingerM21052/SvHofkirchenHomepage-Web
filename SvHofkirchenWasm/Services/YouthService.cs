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
    private readonly IJSRuntime _js;

    public List<MemberDto> YouthMembers { get; private set; } = new();
    public List<PresenceDto> Presences { get; private set; } = new();
    public bool IsInitialized { get; private set; }

    public event Action? OnChange;

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
            // Lädt die Daten ganz normal über HTTPS (sicherer Kanal)
            // Der Server entscheidet, wer was sehen darf
            var root = await _http.GetFromJsonAsync<YouthDataRoot>("api/youth", 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (root != null)
            {
                YouthMembers = root.Members ?? new();
                Presences = root.Presences ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Vereinsdaten: {ex.Message}");
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

    // --- API & BACKUP ---

    private async Task SaveDataAsync()
    {
        try
        {
            // Senden der Daten an den Server (HTTPS verschlüsselt den Transport automatisch)
            // Der Server speichert es im Klartext für Backups & Admin-Einsicht
            var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
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

    public async Task DownloadDataAsync()
    {
        try 
        {
            var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
            string json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);
            
            await _js.InvokeVoidAsync("downloadFileFromStream", "vereinsdaten_backup.json", base64);
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Export Fehler: {ex.Message}");
        }
    }

    public async Task ImportDataAsync(IBrowserFile file)
    {
        try
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 5); 
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            
            var root = JsonSerializer.Deserialize<YouthDataRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (root != null)
            {
                YouthMembers = root.Members ?? new();
                Presences = root.Presences ?? new();
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

    // --- GOOGLE DRIVE BACKUP ---
    public async Task<string> TriggerGoogleDriveBackupAsync()
    {
        try
        {
            // Sendet den Befehl an den Worker (POST /api/backup/drive)
            var response = await _http.PostAsync("api/backup/drive", null);
            
            if (response.IsSuccessStatusCode)
            {
                return "Backup erfolgreich auf Google Drive erstellt!";
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return $"Fehler beim Backup: {errorMsg}";
            }
        }
        catch (Exception ex)
        {
            return $"Verbindungsfehler: {ex.Message}";
        }
    }
}