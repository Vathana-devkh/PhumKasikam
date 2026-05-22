using Microsoft.EntityFrameworkCore;
using Npgsql;
using PhumKasikam.Data; 
using Microsoft.AspNetCore.Authentication.Cookies;

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
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432, 
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require, 
        TrustServerCertificate = true
    };
    connectionString = npgsqlBuilder.ToString();
}

// ==========================================================================
// 🗄️ ផ្នែកកំណត់ការតភ្ជាប់ DATABASE & IGNORE PENDING WARNING (.NET 9)
// ==========================================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });

    options.ConfigureWarnings(warnings => 
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// ==========================================================================
// 🔐 ផ្នែកដំឡើង COOKIE AUTHENTICATION សម្រាប់ ADMIN (បន្ថែមថ្មី)
// ==========================================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login"; // បើមិនទាន់បាន Login ទេ វានឹងរុញដោយស្វ័យប្រវត្តទៅទំព័រនេះ
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // រក្សាទុកការ Login ក្នុងម៉ាស៊ីនរយៈពេល ២ម៉ោង
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// ==========================================================================
// 🧱 ផ្នែកដំឡើង SERVICES ផ្សេងៗ (គាំទ្រទាំង MVC និង RAZOR PAGES)
// ==========================================================================
builder.Services.AddControllersWithViews(); 
builder.Services.AddRazorPages(); 
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ==========================================================================
// 🚀 ផ្នែក AUTOMATED DATABASE MIGRATION (STANDARD PRODUCTION RUN)
// ==========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        Console.WriteLine("==> [PhumKasikam] Running automated database migrations...");
        
        context.Database.SetCommandTimeout(60); 
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

// 💡 លំដាប់សំខាន់បំផុត៖ UseAuthentication ត្រូវតែនៅចន្លោះ UseRouting និង UseAuthorization
app.UseAuthentication(); // 🔒 ពិនិត្យអត្តសញ្ញាណ (អ្នកណាជាអ្នកចូលប្រើ?)
app.UseAuthorization();  // 🔑 ពិនិត្យសិទ្ធិ (តើគណនីនេះមានសិទ្ធិចូលមើលទេ?)

// 💡 ភ្ជាប់ផ្លូវរត់ទាំង MVC និង Razor Pages ឱ្យដើរទន្ទឹមគ្នា
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); 

app.Run();