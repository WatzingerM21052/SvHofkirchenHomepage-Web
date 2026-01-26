using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using SvHofkirchenHomepage;
using SvHofkirchenHomepage.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. HttpClient (für das Laden der lokalen database.json)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 2. Deine Services
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<AuthenticationService>();

// 3. Radzen Services
builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();