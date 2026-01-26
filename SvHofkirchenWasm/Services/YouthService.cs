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
    
    // Version erhöht -> Löscht alten Cache beim Start
    private const string LocalStorageKey = "SvHofkirchen_YouthData_V5_Fix"; 

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
            bool dataLoaded = false;

            // 1. VERSUCH: Browser LocalStorage
            var localData = await _js.InvokeAsync<string>("localStorage.getItem", LocalStorageKey);
            if (!string.IsNullOrEmpty(localData))
            {
                var root = JsonSerializer.Deserialize<YouthDataRoot>(localData);
                if (root != null && root.Members != null && root.Members.Count > 0)
                {
                    YouthMembers = root.Members;
                    Presences = root.Presences ?? new();
                    dataLoaded = true;
                    Console.WriteLine($"[Init] {YouthMembers.Count} Spieler aus LocalStorage geladen.");
                }
            }

            // 2. VERSUCH: youth.json vom Server (nur wenn noch keine Daten)
            if (!dataLoaded)
            {
                try 
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "youth.json");
                    var response = await _http.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var cleanData = await response.Content.ReadFromJsonAsync<YouthDataRoot>();
                        // WICHTIG: Wir akzeptieren die Daten nur, wenn auch wirklich Spieler drin stehen!
                        if (cleanData != null && cleanData.Members != null && cleanData.Members.Count > 0)
                        {
                            YouthMembers = cleanData.Members;
                            Presences = cleanData.Presences ?? new();
                            dataLoaded = true;
                            Console.WriteLine($"[Init] {YouthMembers.Count} Spieler aus youth.json geladen.");
                        }
                        else
                        {
                            Console.WriteLine("[Init] youth.json gefunden, aber leer. Gehe zu Fallback.");
                        }
                    }
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine($"[Init] youth.json konnte nicht geladen werden: {ex.Message}");
                }
            }

            // 3. FALLBACK: database.json (Wenn bisher nichts Sinnvolles gefunden wurde)
            if (!dataLoaded)
            {
                Console.WriteLine("[Init] Starte Fallback-Import aus database.json...");
                try 
                {
                    // Wir nutzen JsonSerializerOptions, um sicherzustellen, dass die Property-Namen passen
                    // (Die Member-Klasse hat [JsonPropertyName], daher sollte default passen)
                    var legacy = await _http.GetFromJsonAsync<LegacyDatabase>("database.json");
                    
                    if (legacy != null && legacy.Member != null)
                    {
                        // 1. Filtern: Nur YouthStatus == 1
                        // 2. Sortieren: Nach Nachname
                        YouthMembers = legacy.Member
                            .Where(m => m.YouthStatus == 1)
                            .OrderBy(m => m.LastName)
                            .ToList();
                        
                        Presences = legacy.YouthPresence ?? new();
                        
                        Console.WriteLine($"[Init] Erfolgreich {YouthMembers.Count} Jugendliche aus database.json importiert!");

                        // Sofort speichern, damit wir beim nächsten Mal Schritt 1 nutzen können
                        await SaveToLocalStorage();
                    }
                    else
                    {
                        Console.WriteLine("[Init] database.json geladen, aber 'Member' Liste war leer oder null.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Init] KRITISCHER FEHLER bei database.json Import: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Init] Unbekannter Fehler: {ex.Message}");
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
        var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
        var json = JsonSerializer.Serialize(root);
        await _js.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, json);
        NotifyStateChanged();
    }

    public async Task DownloadDataAsync()
    {
        var root = new YouthDataRoot { Members = YouthMembers, Presences = Presences };
        var json = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(bytes);
        await _js.InvokeVoidAsync("downloadFileFromStream", "youth.json", base64);
    }

    public async Task ImportDataAsync(IBrowserFile file)
    {
        using var stream = file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 10);
        var root = await JsonSerializer.DeserializeAsync<YouthDataRoot>(stream);
        if (root != null)
        {
            YouthMembers = root.Members ?? new();
            Presences = root.Presences ?? new();
            await SaveToLocalStorage();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}