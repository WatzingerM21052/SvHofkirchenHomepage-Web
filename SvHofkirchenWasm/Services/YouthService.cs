using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using SvHofkirchenWasm.Models;

namespace SvHofkirchenWasm.Services;

public class YouthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private const string LocalStorageKey = "SvHofkirchen_YouthData_V1";

    public List<MemberDto> YouthMembers { get; private set; } = new();
    public List<PresenceDto> Presences { get; private set; } = new();
    public bool IsInitialized { get; private set; }

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
            // 1. Prüfen ob Daten im LocalStorage sind (ungespeicherte Änderungen)
            var localData = await _js.InvokeAsync<string>("localStorage.getItem", LocalStorageKey);
            
            if (!string.IsNullOrEmpty(localData))
            {
                var root = JsonSerializer.Deserialize<YouthDataRoot>(localData);
                if (root != null)
                {
                    YouthMembers = root.Members;
                    Presences = root.Presences;
                    IsInitialized = true;
                    return;
                }
            }

            // 2. Wenn nichts lokal, lade vom Server (bevorzuge youth.json, fallback auf database.json)
            try 
            {
                // Versuch clean file zu laden
                var cleanData = await _http.GetFromJsonAsync<YouthDataRoot>("youth.json");
                YouthMembers = cleanData?.Members ?? new();
                Presences = cleanData?.Presences ?? new();
            }
            catch
            {
                // Fallback: Lade die riesige legacy database.json und filtere
                Console.WriteLine("Lade Legacy-Datenbank...");
                var legacy = await _http.GetFromJsonAsync<LegacyDatabase>("database.json");
                if (legacy != null)
                {
                    // Filter: Nur YouthStatus == 1
                    YouthMembers = legacy.Member.Where(m => m.YouthStatus == 1).ToList();
                    Presences = legacy.YouthPresence;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden: {ex.Message}");
        }

        IsInitialized = true;
    }

    // Toggle Anwesenheit
    public async Task TogglePresenceAsync(int memberId, DateTime date)
    {
        var existing = Presences.FirstOrDefault(p => p.MemberId == memberId && p.ParsedDate.Date == date.Date);

        if (existing != null)
        {
            Presences.Remove(existing);
        }
        else
        {
            Presences.Add(new PresenceDto 
            { 
                MemberId = memberId, 
                ParsedDate = date 
            });
        }
        await SaveToLocalStorage();
    }

    public bool IsPresent(int memberId, DateTime date)
    {
        return Presences.Any(p => p.MemberId == memberId && p.ParsedDate.Date == date.Date);
    }

    // Speichert im Browser Cache
    private async Task SaveToLocalStorage()
    {
        var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
        var json = JsonSerializer.Serialize(root);
        await _js.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, json);
    }

    // Exportiert saubere youth.json
    public async Task DownloadDataAsync()
    {
        var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
        var json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(bytes);
        await _js.InvokeVoidAsync("downloadFileFromStream", "youth.json", base64);
    }
}