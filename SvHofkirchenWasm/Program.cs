using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SvHofkirchenWasm;
using SvHofkirchenWasm.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Services registrieren
builder.Services.AddScoped<YouthService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NavigationService>();

await builder.Build().RunAsync();