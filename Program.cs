using Microsoft.EntityFrameworkCore;
using Npgsql;
using PhumKasikam.Data; // 💡 បានបន្ថែម Namespace របស់លោកអ្នកត្រឹមត្រូវ

var builder = WebApplication.CreateBuilder(args);

// ==========================================================================
// 🛠️ ផ្នែករៀបចំ CONNECTION STRING ឱ្យដើរទាំងលើ LOCAL និង RENDER CLOUD
// ==========================================================================

// 1. ព្យាយាមទាញយកខ្សែភ្ជាប់ពី Environment Variable របស់ Render មុនគេ (URL Format)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrEmpty(connectionString))
{
    // 2. ប្រសិនបើរត់លើ Cloud រកមិនឃើញ (រត់លើ Local) វាទាញពី Environment Variable ទម្រង់ .NET វិញ
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
}

if (string.IsNullOrEmpty(connectionString))
{
    // 3. ប្រសិនបើនៅលើម៉ាស៊ីន Local គ្មាន Environment Variable ទេ វានឹងអានពី appsettings.json ធម្មតា
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// 4. បម្លែងខ្សែភ្ជាប់ (ករណីវាជាទម្រង់ postgresql:// របស់ Render ឱ្យទៅជាទម្រង់ Host=... របស់ .NET)
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
{
    var databaseUri = new Uri(connectionString);
    var userInfo = databaseUri.UserInfo.Split(':');

    var npgsqlBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Username = userInfo[0],
        Password = userInfo.Length > 1 ? userInfo[1] : string.Empty,
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = SslMode.Require, // 🔒 បង្ខំឱ្យប្រើ SSL Mode ជានិច្ចលើ Cloud
        TrustServerCertificate = true
    };
    connectionString = npgsqlBuilder.ToString();
}

// 5. ចាក់ខ្សែភ្ជាប់ចូលទៅក្នុង ApplicationDbContext ពិតប្រាកដរបស់អ្នក
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // កំណត់ឱ្យរត់រាល់ពេលមានបញ្ហា Network បន្តិចបន្តួច (Resiliency)
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    }));

// ==========================================================================
// 🧱 ផ្នែកដំឡើង SERVICES ផ្សេងៗ (STANDARD .NET SERVICES)
// ==========================================================================

builder.Services.AddControllersWithViews(); 
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ==========================================================================
// 🚀 ផ្នែក AUTOMATED DATABASE MIGRATION (កែសម្រួលថ្មីមិនឱ្យគាំង App)
// ==========================================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>(); 
        Console.WriteLine("==> [PhumKasikam] Checking database connection...");
        
        // កំណត់ Timeout ខ្លីត្រឹម ៥ វិនាទី បើភ្ជាប់មិនបានឱ្យវាលោតទៅ catch ភ្លាម មិនឱ្យគាំង App ឡើយ
        context.Database.SetCommandTimeout(5); 
        
        context.Database.Migrate();
        Console.WriteLine("==> [PhumKasikam] Database migration applied successfully!");
    }
    catch (Exception ex)
    {
        // 🔒 កន្លែងសំខាន់៖ បើមាន Error វានឹងហួសទៅមុខទៀត មិនធ្វើឱ្យ App រលំឡើយ
        Console.WriteLine($"==> [PhumKasikam] MIGRATION ERROR (App is still running): {ex.Message}");
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();