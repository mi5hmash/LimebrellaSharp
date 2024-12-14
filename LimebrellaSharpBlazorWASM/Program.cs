using LimebrellaSharpBlazorWASM;
using LimebrellaSharpBlazorWASM.Extensions;
using LimebrellaSharpBlazorWASM.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Builder configuration shorthands
var rootComponents = builder.RootComponents;
var services = builder.Services;

// ROOT COMPONENTS
rootComponents.Add<App>("#app");
rootComponents.Add<HeadOutlet>("head::after");

// SERVICES
// Web Storage Manager
services.AddScoped(sp => new WebStorageManagerService(0xF17A59CB, sp.GetRequiredService<IJSRuntime>()));
// SimpleLoggerWasmService
services.AddScoped<SimpleLoggerWasmService>();
// SuperUser Manager
services.AddScoped<SuperUserService>();
// Theme Manager
services.AddScoped<ThemeManagerService>();
// Language Manager & Localization
services.AddScoped<LanguageManagerService>();
services.AddLocalization();
// HttpClient
services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var host = builder.Build();

// Set the default Culture for Localization
await host.SetDefaultCulture();

await host.RunAsync();