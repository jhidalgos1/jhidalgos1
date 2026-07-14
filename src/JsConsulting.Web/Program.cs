using JsConsulting.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<LocalHistoryStore>();
builder.Services.AddScoped<SqlWorkspaceService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
await app.Services.GetRequiredService<LocalHistoryStore>().InitializeAsync();
app.Run();
