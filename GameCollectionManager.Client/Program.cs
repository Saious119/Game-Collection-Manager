using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GameCollectionManager.Client;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers; // (optional, for HttpClient usage)
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using GameCollectionManager.Client.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Register CustomAuthorizationMessageHandler
builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

// Configure HttpClient with the handler
builder.Services.AddHttpClient("API", client => 
    client.BaseAddress = new Uri("https://localhost:5001"))
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// Add authentication services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProviderV2>();

await builder.Build().RunAsync();
