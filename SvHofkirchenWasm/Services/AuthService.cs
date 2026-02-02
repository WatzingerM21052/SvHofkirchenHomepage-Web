using System.Text.Json;
using SvHofkirchenWasm.Models;

namespace SvHofkirchenWasm.Services;

public class AuthService
{
    private const string AuthStateKey = "auth_state";
    private const string UserKey = "current_user";

    private User? _currentUser;
    private bool _isAuthenticated = false;

    public event Action? OnAuthStateChanged;

    public AuthService()
    {
        LoadAuthState();
    }

    public async Task<AuthResponse> LoginAsync(string userName, string password)
    {
        try
        {
            // TODO: API-Call zur Cloudflare Worker durchführen
            // Für jetzt: Lokale Demo-Authentifizierung

            // Demo: Nur "admin" / "admin" funktioniert
            if (userName == "admin" && password == "admin")
            {
                _currentUser = new User
                {
                    UserId = 1,
                    UserName = "admin",
                    Email = "schachverein.hofkirchen@gmx.at",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow
                };

                _isAuthenticated = true;
                SaveAuthState();
                NotifyAuthStateChanged();

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login erfolgreich",
                    User = _currentUser
                };
            }

            return new AuthResponse
            {
                Success = false,
                Message = "Ungültige Anmeldedaten"
            };
        }
        catch (Exception ex)
        {
            return new AuthResponse
            {
                Success = false,
                Message = $"Login-Fehler: {ex.Message}"
            };
        }
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _isAuthenticated = false;
        ClearAuthState();
        NotifyAuthStateChanged();
    }

    public bool IsAuthenticated => _isAuthenticated;

    public User? CurrentUser => _currentUser;

    public string? CurrentRole => _currentUser?.Role;

    private void SaveAuthState()
    {
        if (_currentUser != null)
        {
            var json = JsonSerializer.Serialize(_currentUser);
            localStorage.SetItem(UserKey, json);
            localStorage.SetItem(AuthStateKey, "true");
        }
    }

    private void LoadAuthState()
    {
        try
        {
            var authState = localStorage.GetItem(AuthStateKey);
            if (authState == "true")
            {
                var userJson = localStorage.GetItem(UserKey);
                if (!string.IsNullOrEmpty(userJson))
                {
                    _currentUser = JsonSerializer.Deserialize<User>(userJson);
                    _isAuthenticated = _currentUser != null;
                }
            }
        }
        catch
        {
            // LocalStorage nicht verfügbar oder Fehler
        }
    }

    private void ClearAuthState()
    {
        try
        {
            localStorage.RemoveItem(AuthStateKey);
            localStorage.RemoveItem(UserKey);
        }
        catch
        {
            // LocalStorage nicht verfügbar
        }
    }

    private void NotifyAuthStateChanged()
    {
        OnAuthStateChanged?.Invoke();
    }
}

// Helper für LocalStorage-Zugriff (wird in der Komponente verwendet)
public static class localStorage
{
    private static bool _initialized = false;
    private static Dictionary<string, string> _storage = new();

    public static void SetItem(string key, string value)
    {
        _storage[key] = value;
    }

    public static string? GetItem(string key)
    {
        return _storage.ContainsKey(key) ? _storage[key] : null;
    }

    public static void RemoveItem(string key)
    {
        _storage.Remove(key);
    }
}
