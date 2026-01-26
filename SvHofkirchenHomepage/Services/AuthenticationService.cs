using SvHofkirchenHomepage.Models;

namespace SvHofkirchenHomepage.Services;

public class AuthenticationService
{
    private bool _isAuthenticated = false;
    public event Action? OnChange;
    
    private readonly DataService _dataService;

    public AuthenticationService(DataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<bool> IsAuthenticated()
    {
        return await Task.FromResult(_isAuthenticated);
    }

    public async Task<bool> Login(string username, string password)
    {
        // DEMO LOGIN: Prüft gegen die JSON-Datenbank oder Admin/Admin
        var data = await _dataService.GetDataAsync();
        var user = data.User?.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));

        // ACHTUNG: Hier keine echten Hash-Checks für Demo, sondern einfacher Check
        // oder einfach Admin/Admin erlauben
        if ((username == "admin" && password == "admin") || user != null) 
        {
            _isAuthenticated = true;
            NotifyStateChanged();
            return true;
        }

        return false;
    }

    public async Task Logout()
    {
        _isAuthenticated = false;
        NotifyStateChanged();
        await Task.CompletedTask;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}