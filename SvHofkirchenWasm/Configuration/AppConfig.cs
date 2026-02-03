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
        if (isDevelopment)
            return "/";
        
        return "/SvHofkirchenHomepage-Web/";
    }

    /// <summary>
    /// Generiert die Home URL basierend auf dem aktuellen Kontext
    /// </summary>
    public static string GetHomeUrl()
    {
        // Einfache Logik: Wenn wir nicht sicher sind, nehmen wir den relativen Pfad
        return "/";
    }

    /// <summary>
    /// API Base URL für Cloudflare Worker
    /// </summary>
    public static string ApiBaseUrl => "https://svhofkirchen-api.svhofkirchen-api.workers.dev";
}