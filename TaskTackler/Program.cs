using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskTackler;
using TaskTackler.Cache;
using TaskTackler.Handlers;
using TaskTackler.Models;
using TaskTackler.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped<CachingHandler>();

var apiUrl = Environment.GetEnvironmentVariable("ApiUrl")
             ?? builder.Configuration.GetValue<string>("ApiUrl")
             ?? "https://localhost:7213/api/";

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri(apiUrl);
}).AddHttpMessageHandler<AuthorizationMessageHandler>()
.AddHttpMessageHandler<CachingHandler>();

builder.Services.AddScoped<TokenModel>();
builder.Services.AddScoped<CacheManager>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();


await builder.Build().RunAsync();
