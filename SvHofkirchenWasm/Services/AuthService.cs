using System.Net.Http.Json;
using System.Text.Json;
using SvHofkirchenWasm.Models;

namespace SvHofkirchenWasm.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private User? _currentUser;
    private bool _isAuthenticated = false;

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task InitializeAsync()
    {
        // Hier könnte man später prüfen, ob der User noch eingeloggt ist.
        // Für den Moment starten wir immer ausgeloggt.
        await Task.CompletedTask;
    }

    public async Task<AuthResponse> LoginAsync(string userName, string password)
    {
        try
        {
            var loginData = new { Username = userName, Password = password };
            
            // Echter API Call an Cloudflare
            var response = await _http.PostAsJsonAsync("api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                
                if (result != null && result.Success)
                {
                    _currentUser = result.User;
                    _isAuthenticated = true;
                    NotifyAuthStateChanged();
                    return result;
                }
            }

            return new AuthResponse { Success = false, Message = "Benutzername oder Passwort falsch." };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login Fehler: {ex.Message}");
            return new AuthResponse { Success = false, Message = "Server nicht erreichbar." };
        }
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _isAuthenticated = false;
        NotifyAuthStateChanged();
        await Task.CompletedTask;
    }

    public bool IsAuthenticated => _isAuthenticated;
    public User? CurrentUser => _currentUser;
    public string? CurrentRole => _currentUser?.Role;

    private void NotifyAuthStateChanged() => OnAuthStateChanged?.Invoke();
}