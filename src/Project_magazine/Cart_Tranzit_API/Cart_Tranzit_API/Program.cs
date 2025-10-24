
using Microsoft.AspNetCore.Authentication.Negotiate;
using ConfigLibrary;

var builder = WebApplication.CreateBuilder(args);
var configFilePath = Path.Combine(AppContext.BaseDirectory, "Cart_Transit_API", "ConfigManager", "AppConfig.json");

var appPort = builder.Configuration.GetValue<int>("AppConfig:AppPort");


builder.Services.AddHttpClient();
builder.Services.AddSingleton<ConfigManager>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();
builder.WebHost.UseUrls($"https://localhost:{appPort}");

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
	options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseRouting();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllers();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();
