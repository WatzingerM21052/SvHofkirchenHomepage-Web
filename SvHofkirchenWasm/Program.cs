using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SvHofkirchenWasm;
using SvHofkirchenWasm.Services;
using SvHofkirchenWasm.Configuration;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. HttpClient auf die Cloudflare API konfigurieren
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(AppConfig.ApiBaseUrl) 
});

// 2. Services registrieren
builder.Services.AddScoped<YouthService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NavigationService>();

await builder.Build().RunAsync();