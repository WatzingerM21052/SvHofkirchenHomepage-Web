using System.Net.Http.Json;
using SvHofkirchenHomepage.Models;

namespace SvHofkirchenHomepage.Services;

public class DataService
{
    private readonly HttpClient _http;
    private DatabaseRoot? _cachedData;

    public DataService(HttpClient http)
    {
        _http = http;
    }

    public async Task<DatabaseRoot> GetDataAsync()
    {
        if (_cachedData != null) return _cachedData;

        try 
        {
            // Lädt die Datei physisch aus wwwroot/data/
            _cachedData = await _http.GetFromJsonAsync<DatabaseRoot>("data/database.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL: Konnte Datenbank nicht laden. {ex.Message}");
            _cachedData = new DatabaseRoot();
        }

        return _cachedData ?? new DatabaseRoot();
    }
}