using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskTackler;
using TaskTackler.Models;
using TaskTackler.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthorizationMessageHandler>();


builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7213/api/");
}).AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped<TokenModel>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();


await builder.Build().RunAsync();
