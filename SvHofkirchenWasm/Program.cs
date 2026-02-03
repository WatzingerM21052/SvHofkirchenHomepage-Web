using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SvHofkirchenWasm;
using SvHofkirchenWasm.Services;
using SvHofkirchenWasm.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// WICHTIG: HttpClient auf die Cloudflare API zeigen lassen
// Wenn AppConfig.ApiBaseUrl leer ist, nutzen wir die Cloudflare URL direkt
var apiUrl = !string.IsNullOrEmpty(AppConfig.ApiBaseUrl) 
             ? AppConfig.ApiBaseUrl 
             : "https://svhofkirchen-api.svhofkirchen-api.workers.dev";

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiUrl) 
});

// Services registrieren
builder.Services.AddScoped<YouthService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NavigationService>();

await builder.Build().RunAsync();