using API_bacup_server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSingleton<DumpControl>();
builder.Configuration.AddJsonFile("C:\\Users\\alex\\source\repos\\API_bacup_server\\API_bacup_server\\MyConfigRep\\AppConfig\\AppConfig.json", optional: false, reloadOnChange: true);
builder.Services.Configure<Config>(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
