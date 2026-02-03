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
    /// API Base URL für Cloudflare Worker
    /// </summary>
    public static string ApiBaseUrl => "https://sv-hofkirchen-api.example.workers.dev/";
}
