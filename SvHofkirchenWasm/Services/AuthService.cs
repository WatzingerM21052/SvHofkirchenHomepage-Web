using System.Text.Json;
using SvHofkirchenWasm.Models;
using Microsoft.JSInterop;

namespace SvHofkirchenWasm.Services;

public class AuthService
{
    private const string AuthStateKey = "auth_state";
    private const string UserKey = "current_user";

    private User? _currentUser;
    private bool _isAuthenticated = false;
    private readonly IJSRuntime _jsRuntime;
    private LocalStorageService _storageService;

    public event Action? OnAuthStateChanged;

    public AuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _storageService = new LocalStorageService(_jsRuntime);
        // LoadAuthState wird nach Komponenten-Init aufgerufen
    }

    public async Task InitializeAsync()
    {
        await LoadAuthStateAsync();
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
                await SaveAuthStateAsync();
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
        await ClearAuthStateAsync();
        NotifyAuthStateChanged();
    }

    public bool IsAuthenticated => _isAuthenticated;

    public User? CurrentUser => _currentUser;

    public string? CurrentRole => _currentUser?.Role;

    private async Task SaveAuthStateAsync()
    {
        if (_currentUser != null)
        {
            try
            {
                var json = JsonSerializer.Serialize(_currentUser);
                await _storageService.SetItemAsync(UserKey, json);
                await _storageService.SetItemAsync(AuthStateKey, "true");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving auth state: {ex.Message}");
            }
        }
    }

    private async Task LoadAuthStateAsync()
    {
        try
        {
            var authState = await _storageService.GetItemAsync(AuthStateKey);
            if (authState == "true")
            {
                var userJson = await _storageService.GetItemAsync(UserKey);
                if (!string.IsNullOrEmpty(userJson))
                {
                    _currentUser = JsonSerializer.Deserialize<User>(userJson);
                    _isAuthenticated = _currentUser != null;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading auth state: {ex.Message}");
        }
    }

    private async Task ClearAuthStateAsync()
    {
        try
        {
            await _storageService.RemoveItemAsync(AuthStateKey);
            await _storageService.RemoveItemAsync(UserKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing auth state: {ex.Message}");
        }
    }

    private void NotifyAuthStateChanged()
    {
        OnAuthStateChanged?.Invoke();
    }
}

// LocalStorage Service mit JS Interop
public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _storageAvailable = false;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync(string key, string value)
    {
        try
        {
            _storageAvailable = await IsAvailableAsync();
            if (!_storageAvailable) return;

            await _jsRuntime.InvokeAsync<bool>("storageHelper.setItem", key, value);
        }
        catch (Exception ex)
        {
            _storageAvailable = false;
            Console.WriteLine($"LocalStorage SetItem failed: {ex.Message}");
        }
    }

    public async Task<string?> GetItemAsync(string key)
    {
        try
        {
            _storageAvailable = await IsAvailableAsync();
            if (!_storageAvailable) return null;

            return await _jsRuntime.InvokeAsync<string?>("storageHelper.getItem", key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LocalStorage GetItem failed: {ex.Message}");
            return null;
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            _storageAvailable = await IsAvailableAsync();
            if (!_storageAvailable) return;

            await _jsRuntime.InvokeAsync<bool>("storageHelper.removeItem", key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LocalStorage RemoveItem failed: {ex.Message}");
        }
    }

    private async Task<bool> IsAvailableAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("storageHelper.isAvailable");
        }
        catch
        {
            return false;
        }
    }
}
