using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskTackler;
using TaskTackler.Handlers;
using TaskTackler.Models;
using TaskTackler.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped<CashingHandler>();


builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7213/api/");
}).AddHttpMessageHandler<AuthorizationMessageHandler>()
.AddHttpMessageHandler<CashingHandler>();

builder.Services.AddScoped<TokenModel>();
builder.Services.AddScoped<CacheManager>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();


await builder.Build().RunAsync();
