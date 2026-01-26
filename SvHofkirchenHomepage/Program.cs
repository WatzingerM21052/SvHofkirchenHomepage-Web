using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using SvHofkirchenHomepage; 
using SvHofkirchenHomepage.Components; // FIX: Wichtig, damit 'App' gefunden wird
using SvHofkirchenHomepage.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Hier gab es den Fehler, jetzt sollte 'App' durch das using oben gefunden werden
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<AuthenticationService>();

builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();