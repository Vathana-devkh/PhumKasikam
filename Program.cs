using Microsoft.EntityFrameworkCore;
using Npgsql;
using PhumKasikam.Data; 

var builder = WebApplication.CreateBuilder(args);

// ==========================================================================
// 🔒 🛠️ ដំណោះស្រាយបញ្ហា INOTIFY LIMIT (STATUS 139) លើ RENDER CLOUD
// ==========================================================================
builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables();

// ==========================================================================
// 🛠️ ផ្នែករៀបចំ CONNECTION STRING ឱ្យដើរទាំងលើ LOCAL និង RENDER CLOUD
// ==========================================================================

var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
}

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
{
    var databaseUri = new Uri(connectionString);
    var userInfo = databaseUri.UserInfo.Split(':');

    var npgsqlBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        // 💡 កែតម្រូវត្រង់នេះ៖ ប្រសិនបើ Port ស្មើ -1 (មិនបានបញ្ជាក់) សូមប្រើ 5432 ជំនួសវិញ
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432, 
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require, 
        TrustServerCertificate = true
    };
    connectionString = npgsqlBuilder.ToString();
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    }));

// ==========================================================================
// 🧱 ផ្នែកដំឡើង SERVICES ផ្សេងៗ (គាំទ្រទាំង MVC និង RAZOR PAGES)
// ==========================================================================

builder.Services.AddControllersWithViews(); 
builder.Services.AddRazorPages(); // 💡 បើកដំណើរការ Razor Pages សម្រាប់ការបង្ហាញទំព័រ
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ==========================================================================
// 🚀 ផ្នែក AUTOMATED DATABASE MIGRATION
// ==========================================================================

// 🚀 ផ្នែក AUTOMATED DATABASE MIGRATION
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine("==> [PhumKasikam] Checking and fixing database structure...");
        
        // ⚠️ ការពារករណី Error 42704 (datetime type does not exist)
        // យើងលុបតារាង Blogs ចោលប្រសិនបើវាមានបញ្ហា ដើម្បីឱ្យ EF Core សង់ថ្មីដោយប្រើ timestamp
        context.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS \"Blogs\" CASCADE;");
        
        context.Database.SetCommandTimeout(30); 
        context.Database.Migrate();
        
        Console.WriteLine("==> [PhumKasikam] Database migration applied successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"==> [PhumKasikam] MIGRATION ERROR: {ex.Message}");
    }
}

// ==========================================================================
// 🌐 HTTP REQUEST PIPELINE (MIDDLEWARES)
// ==========================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 💡 ភ្ជាប់ផ្លូវរត់ទាំង MVC និង Razor Pages ឱ្យដើរទន្ទឹមគ្នា
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // 🔒 បើកដំណើរការ Routing សម្រាប់ Folder Pages

app.Run();