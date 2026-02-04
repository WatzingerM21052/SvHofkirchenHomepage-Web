using System.Net.Http.Json;
using System.Text.Json;
using SvHofkirchenWasm.Models;

namespace SvHofkirchenWasm.Services;

public class AuthService
{
    private readonly HttpClient _http;
    
    private User? _currentUser;
    private bool _isAuthenticated = false;
    private List<User> _users = new();
    private bool _usersLoaded = false;

    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task InitializeAsync() => await Task.CompletedTask;

    public async Task<AuthResponse> LoginAsync(string userName, string password)
    {
        try
        {
            // Login Daten werden per HTTPS verschlüsselt gesendet
            var response = await _http.PostAsJsonAsync("api/auth/login", new { Username = userName, Password = password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponse>(_options);
                if (result != null && result.Success)
                {
                    _currentUser = result.User;
                    _isAuthenticated = true;
                    NotifyAuthStateChanged();
                    return result;
                }
            }
            return new AuthResponse { Success = false, Message = "Login fehlgeschlagen" };
        }
        catch (Exception) { return new AuthResponse { Success = false, Message = "Verbindungsfehler" }; }
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;
        _isAuthenticated = false;
        NotifyAuthStateChanged();
        await Task.CompletedTask;
    }

    // --- USER MANAGEMENT ---

    public async Task<List<User>> GetUsersAsync()
    {
        try 
        {
            var result = await _http.GetFromJsonAsync<List<User>>("api/users", _options);
            if (result != null) 
            {
                _users = result;
                _usersLoaded = true;
            }
        }
        catch (Exception ex) { Console.WriteLine("Ladefehler: " + ex.Message); }
        return _users;
    }

    public async Task AddUserAsync(User user)
    {
        if (!_usersLoaded) await GetUsersAsync();

        if (_users.Any(u => u.UserName.Equals(user.UserName, StringComparison.OrdinalIgnoreCase))) return;

        user.UserId = _users.Any() ? _users.Max(u => u.UserId) + 1 : 1;
        _users.Add(user);
        await SaveUsersToApiAsync();
    }

    public async Task UpdateUserAsync(User updatedUser)
    {
        if (!_usersLoaded) await GetUsersAsync();

        var index = _users.FindIndex(u => u.UserId == updatedUser.UserId);
        if (index != -1)
        {
            if (string.IsNullOrEmpty(updatedUser.Password))
            {
                updatedUser.Password = _users[index].Password;
            }
            _users[index] = updatedUser;
            await SaveUsersToApiAsync();
        }
    }

    public async Task DeleteUserAsync(int userId)
    {
        if (!_usersLoaded) await GetUsersAsync();
        var user = _users.FirstOrDefault(u => u.UserId == userId);
        if (user != null)
        {
            _users.Remove(user);
            await SaveUsersToApiAsync();
        }
    }

    private async Task SaveUsersToApiAsync()
    {
        // Users werden im Klartext (JSON) an den Server gesendet, aber durch HTTPS Tunnel geschützt.
        // Der Server speichert sie lesbar, damit Admin sie bearbeiten kann.
        try { await _http.PostAsJsonAsync("api/users", _users); }
        catch (Exception ex) { Console.WriteLine("Speicherfehler: " + ex.Message); }
    }

    public bool IsAuthenticated => _isAuthenticated;
    public User? CurrentUser => _currentUser;
    public string? CurrentRole => _currentUser?.Role;
    private void NotifyAuthStateChanged() => OnAuthStateChanged?.Invoke();
}