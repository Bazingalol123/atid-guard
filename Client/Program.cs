using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AuthWithAdmin.Client;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using AuthWithAdmin.Client.ClientHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//Local Storage
builder.Services.AddBlazoredLocalStorage();

//User management
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

//Popup Service - registered as scoped to access JSRuntime
builder.Services.AddScoped<PopupService>();
builder.Services.AddScoped<JsConfirmService>();

//Enable detailed logging in development
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("VerifiedUser", policy =>
        policy.RequireClaim("IsVerified", "true"));
});

await builder.Build().RunAsync();