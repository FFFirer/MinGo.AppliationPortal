using AppPortal.Services;
using LiteDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// LiteDB
var dbPath = builder.Configuration.GetValue<string>("LiteDb:Path") ?? "AppPortal.db";
builder.Services.AddSingleton(new LiteDatabase(dbPath));
builder.Services.AddScoped<IAppService, AppService>();
builder.Services.AddScoped<IHostService, HostService>();
builder.Services.AddScoped<IDataSourceService, DataSourceService>();
builder.Services.AddScoped<IPortainerClient, PortainerClient>();
builder.Services.AddScoped<IAppSource, PortainerAppSource>();
builder.Services.AddScoped<IImportService, ImportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
