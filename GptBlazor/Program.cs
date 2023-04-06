using GptBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using GptBlazor.Data;
using Microsoft.Extensions.Primitives;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using App = Furion.App;

var builder = WebApplication.CreateBuilder(args).Inject();

// Add services to the container.
builder.Services.AddRazorPages().AddInjectBase();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddBootstrapBlazor();
builder.Services.AddHotKeys2();

var config = App.Configuration.GetSection("ChatGpt");
Constant.ResourceName = config["ResourceName"];
Constant.DeploymentId = config["DeploymentId"];
Constant.ApiKey = config["ApiKey"];
ChangeToken.OnChange(() => config.GetReloadToken(), () =>
{
    Constant.ResourceName = config["ResourceName"];
    Constant.DeploymentId = config["DeploymentId"];
    Constant.ApiKey = config["ApiKey"];
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseInjectBase();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();