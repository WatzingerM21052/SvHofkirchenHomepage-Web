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
    }

    public async Task InitializeAsync()
    {
        await LoadAuthStateAsync();
    }

    public async Task<AuthResponse> LoginAsync(string userName, string password)
    {
        try
        {
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

// LocalStorage Service mit JS Interop und robustem Fallback
public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    
    // In-Memory Fallback Speicher, falls LocalStorage blockiert ist
    private readonly Dictionary<string, string> _memoryStore = new Dictionary<string, string>();
    private bool _useMemoryFallback = false;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync(string key, string value)
    {
        if (_useMemoryFallback)
        {
            _memoryStore[key] = value;
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LocalStorage Zugriff verweigert ({ex.Message}). Nutze nun In-Memory-Speicher.");
            _useMemoryFallback = true;
            _memoryStore[key] = value;
        }
    }

    public async Task<string?> GetItemAsync(string key)
    {
        if (_useMemoryFallback)
        {
            return _memoryStore.TryGetValue(key, out var val) ? val : null;
        }

        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LocalStorage Zugriff verweigert ({ex.Message}). Nutze nun In-Memory-Speicher.");
            _useMemoryFallback = true;
            return _memoryStore.TryGetValue(key, out var val) ? val : null;
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        if (_useMemoryFallback)
        {
            _memoryStore.Remove(key);
            return;
        }

        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LocalStorage Remove fehlgeschlagen: {ex.Message}");
            _useMemoryFallback = true;
            _memoryStore.Remove(key);
        }
    }
}