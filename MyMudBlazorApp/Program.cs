using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyMudBlazorApp;
using MyMudBlazorApp.Services;
using MudBlazor.Services;
using Blazored.LocalStorage; 

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5000") });
builder.Services.AddMudServices();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddBlazoredLocalStorage(); 

await builder.Build().RunAsync();
