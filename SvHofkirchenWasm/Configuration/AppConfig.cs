namespace SvHofkirchenWasm.Configuration;

/// <summary>
/// Zentrale Konfiguration für die Anwendung
/// </summary>
public static class AppConfig
{
    /// <summary>
    /// Base Path für lokale Entwicklung: "/"
    /// Base Path für GitHub Pages: "/SvHofkirchenHomepage-Web/"
    /// </summary>
    public static string GetBasePath(bool isDevelopment)
    {
        // Für lokale Entwicklung "/" verwenden
        if (isDevelopment)
            return "/";
        
        // Für Production (GitHub Pages) mit Subfolder
        return "/SvHofkirchenHomepage-Web/";
    }

    /// <summary>
    /// Generiert die Home URL basierend auf dem aktuellen Kontext
    /// </summary>
    public static string GetHomeUrl()
    {
        var isLocalhost = IsLocalhost();
        
        if (isLocalhost)
        {
            // Für localhost: nur die relative URL
            return "/";
        }
        else
        {
            // Für GitHub Pages: absolute URL
            return "https://watzingerm21052.github.io/SvHofkirchenHomepage-Web/";
        }
    }

    /// <summary>
    /// Prüft, ob die App auf localhost läuft
    /// </summary>
    public static bool IsLocalhost()
    {
        // Diese Methode wird von JavaScript aus abgerufen
        // Im C# Code wird diese Info vom browser.location bereitgestellt
        return false; // Default, wird nur von JS verwendet
    }

    /// <summary>
    /// API Base URL für Cloudflare Worker
    /// </summary>
    public static string ApiBaseUrl => "https://sv-hofkirchen-api.example.workers.dev/";
}
