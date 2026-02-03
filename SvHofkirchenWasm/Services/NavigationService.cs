using Microsoft.AspNetCore.Components;
using SvHofkirchenWasm.Configuration;

namespace SvHofkirchenWasm.Services;

/// <summary>
/// Service für zentrale Navigation mit automatischer Anpassung an Dev/Production
/// </summary>
public class NavigationService
{
    private readonly NavigationManager _navigationManager;

    public NavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Bestimmt, ob die App auf localhost läuft
    /// </summary>
    private bool IsLocalhost()
    {
        var uri = _navigationManager.Uri;
        return uri.Contains("localhost") || uri.Contains("127.0.0.1");
    }

    /// <summary>
    /// Navigiert zur Home-Seite (dynamic je nach Kontext)
    /// </summary>
    public void NavigateToHome(bool forceLoad = false)
    {
        var homeUrl = GetHomeUrl();
        _navigationManager.NavigateTo(homeUrl, forceLoad);
    }

    /// <summary>
    /// Navigiert zu einer relativen Route (wird automatisch mit base path kombiniert)
    /// </summary>
    public void NavigateTo(string route)
    {
        _navigationManager.NavigateTo(route);
    }

    /// <summary>
    /// Generiert die Home URL basierend auf aktueller Umgebung
    /// </summary>
    public string GetHomeUrl()
    {
        if (IsLocalhost())
        {
            // Für localhost: relative URL
            return "/";
        }
        else
        {
            // Für GitHub Pages: absolute URL
            return "https://watzingerm21052.github.io/SvHofkirchenHomepage-Web/";
        }
    }

    /// <summary>
    /// Gibt den aktuellen Umgebungskontext zurück (für Logging/Debugging)
    /// </summary>
    public string GetEnvironmentInfo()
    {
        var isLocalhost = IsLocalhost();
        return isLocalhost ? "Development (localhost)" : "Production (GitHub Pages)";
    }
}
