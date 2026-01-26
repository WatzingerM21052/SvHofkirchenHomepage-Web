// Services/DataService.cs
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using SvHofkirchenWasm.Models;

namespace SvHofkirchenWasm.Services;

public class DataService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private DatabaseRoot _data = new();
    private const string LocalStorageKey = "SvHofkirchenData_V1";

    public DataService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public DatabaseRoot Data => _data;
    public bool IsInitialized { get; private set; }

    // Initialisiert die App: Versucht erst LocalStorage, dann die Server-JSON
    public async Task InitializeAsync()
    {
        if (IsInitialized) return;

        try
        {
            var localDataJson = await _js.InvokeAsync<string>("localStorage.getItem", LocalStorageKey);
            
            if (!string.IsNullOrEmpty(localDataJson))
            {
                _data = JsonSerializer.Deserialize<DatabaseRoot>(localDataJson) ?? new DatabaseRoot();
                Console.WriteLine("Daten aus LocalStorage geladen.");
            }
            else
            {
                _data = await _http.GetFromJsonAsync<DatabaseRoot>("database.json") ?? new DatabaseRoot();
                Console.WriteLine("Daten aus database.json geladen.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden: {ex.Message}");
        }

        IsInitialized = true;
    }

    // Speichert Änderungen im Browser des Nutzers
    public async Task SaveChangesAsync()
    {
        var json = JsonSerializer.Serialize(_data);
        await _js.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, json);
    }

    // Exportiert die aktuelle Datenbank als JSON Datei für GitHub
    public async Task DownloadDatabaseAsync()
    {
        var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
        var fileName = "database.json";
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(bytes);
        
        await _js.InvokeVoidAsync("downloadFileFromStream", fileName, base64);
    }
}