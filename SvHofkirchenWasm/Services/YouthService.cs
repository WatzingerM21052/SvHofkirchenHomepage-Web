using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SvHofkirchenWasm.Models;

namespace SvHofkirchenWasm.Services;

public class YouthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    
    // Version erhöht -> Erzwingt Neu-Laden der Daten beim nächsten Start
    private const string LocalStorageKey = "SvHofkirchen_YouthData_V6_AgeFix"; 

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
            string? localData = null;
            if (await IsStorageAvailableAsync())
            {
                localData = await _js.InvokeAsync<string?>("storageHelper.getItem", LocalStorageKey);
            }
            if (!string.IsNullOrEmpty(localData))
            {
                var root = JsonSerializer.Deserialize<YouthDataRoot>(localData);
                if (root != null && root.Members != null && root.Members.Count > 0)
                {
                    YouthMembers = root.Members;
                    Presences = root.Presences ?? new();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden der Jugenddaten: {ex.Message}");
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
        await SaveToLocalStorage();
    }

    public async Task UpdateMemberAsync(MemberDto updatedMember)
    {
        var existing = YouthMembers.FirstOrDefault(m => m.MemberId == updatedMember.MemberId);
        if (existing != null)
        {
            existing.FirstName = updatedMember.FirstName;
            existing.LastName = updatedMember.LastName;
            existing.BirthDateString = updatedMember.BirthDateString;
            await SaveToLocalStorage();
        }
    }

    public async Task DeleteMemberAsync(int memberId)
    {
        var member = YouthMembers.FirstOrDefault(m => m.MemberId == memberId);
        if (member != null)
        {
            YouthMembers.Remove(member);
            Presences.RemoveAll(p => p.MemberId == memberId);
            await SaveToLocalStorage();
        }
    }

    // --- ANWESENHEIT ---
    public async Task TogglePresenceAsync(int memberId, DateTime date)
    {
        var existing = Presences.FirstOrDefault(p => p.MemberId == memberId && p.ParsedDate.Date == date.Date);
        if (existing != null) Presences.Remove(existing);
        else Presences.Add(new PresenceDto { MemberId = memberId, ParsedDate = date });
        await SaveToLocalStorage();
    }

    public bool IsPresent(int memberId, DateTime date)
    {
        return Presences.Any(p => p.MemberId == memberId && p.ParsedDate.Date == date.Date);
    }

    // --- DATA HANDLING ---
    private async Task SaveToLocalStorage()
    {
        try 
        {
            if (await IsStorageAvailableAsync())
            {
                var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
                var json = JsonSerializer.Serialize(root);
                await _js.InvokeAsync<bool>("storageHelper.setItem", LocalStorageKey, json);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Speichern fehlgeschlagen (Ignoriert): {ex.Message}");
        }
        
        NotifyStateChanged();
    }

    public async Task DownloadDataAsync()
    {
        try 
        {
            var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
            var json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var base64 = Convert.ToBase64String(bytes);
            await _js.InvokeVoidAsync("downloadFileFromStream", "youth.json", base64);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Download fehlgeschlagen: {ex.Message}");
        }
    }

    public async Task ImportDataAsync(IBrowserFile file)
    {
        using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 10);
        
        try
        {
            var root = await JsonSerializer.DeserializeAsync<YouthDataRoot>(stream);
            if (root != null && root.Members != null && root.Members.Count > 0)
            {
                YouthMembers = root.Members;
                Presences = root.Presences ?? new();
                await SaveToLocalStorage();
                return;
            }
        }
        catch
        {
            try {
                if(stream.CanSeek) stream.Position = 0;
            } catch { }
        }
        
        try
        {
            var legacy = await JsonSerializer.DeserializeAsync<LegacyDatabase>(stream);
            if (legacy != null && legacy.Member != null)
            {
                YouthMembers = legacy.Member.Where(m => m.YouthStatus == 1).ToList();
                Presences = legacy.YouthPresence ?? new();
                await SaveToLocalStorage();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Import-Fehler: {ex.Message}");
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    private async Task<bool> IsStorageAvailableAsync()
    {
        try
        {
            return await _js.InvokeAsync<bool>("storageHelper.isAvailable");
        }
        catch
        {
            return false;
        }
    }
}