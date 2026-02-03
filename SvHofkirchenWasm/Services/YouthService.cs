using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using SvHofkirchenWasm.Models;

namespace SvHofkirchenWasm.Services;

public class YouthService
{
    private readonly HttpClient _http;
    
    public List<MemberDto> YouthMembers { get; private set; } = new();
    public List<PresenceDto> Presences { get; private set; } = new();
    public bool IsInitialized { get; private set; }

    public event Action? OnChange;

    public YouthService(HttpClient http)
    {
        _http = http;
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
        // Einfache ID-Generierung für die UI
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

    // --- API HANDLING ---
    private async Task SaveDataAsync()
    {
        try
        {
            var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
            
            // POST Request an Cloudflare Worker (Speichern)
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
        // Funktionalität für manuellen Download, falls gewünscht, sonst leer lassen
        NotifyStateChanged(); 
    }

    public async Task ImportDataAsync(IBrowserFile file)
    {
        // Import-Logik hier entfernen oder anpassen, falls Upload gewünscht ist
        await Task.CompletedTask;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}