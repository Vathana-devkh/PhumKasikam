using Microsoft.EntityFrameworkCore;
// សូមប្តូរតួអក្សរខាងក្រោមទៅតាម Namespace ពិតប្រាកដនៃគម្រោងរបស់លោកអ្នក (ឧទាហរណ៍៖ PhumKasikam.Data)
using PhumKasikam.Data; 

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURING SERVICES (Dependency Injection)
// ==========================================

// បន្ថែមសេវាកម្ម Razor Pages ទៅក្នុងប្រព័ន្ធ (ដោះស្រាយបញ្ហា Error 404)
builder.Services.AddRazorPages();

// ទាញយក Connection String ពី Environment Variable របស់ Render (ឈ្មោះ CONNECTION_STRING)
// ប្រសិនបករកមិនឃើញនៅលើ Server វានឹងទាញចេញពី appsettings.json ជំនួសវិញ
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// កំណត់ការតភ្ជាប់ទៅកាន់ PostgreSQL Database តាមរយៈ Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // កំណត់ឱ្យ EF Core ព្យាយាមតភ្ជាប់ឡើងវិញដោយស្វ័យប្រវត្តិនៅពេលដាច់ Network ដុំកំភួន
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    }));

var app = builder.Build();

// ==========================================
// 2. DATABASE AUTOMATED MIGRATIONS
// ==========================================

// កូដស្វ័យប្រវត្តិសម្រាប់រត់ Migration ភ្លាមៗនៅពេលដែល Application ចាប់ផ្ដើមដំណើរការ (Startup)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // ពិនិត្យ និងរត់ Migration ដែលនៅសេសសល់ទាំងអស់ចូល Database ដោយស្វ័យប្រវត្តិ
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// ==========================================
// 3. MIDDLEWARE PIPELINE (HTTP Request)
// ==========================================

// កំណត់លក្ខខណ្ឌសុវត្ថិភាព និងការបង្ហាញកំហុសទៅតាម Environment
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // កំណត់ឱ្យប្រើប្រាស់ HSTS (HTTP Strict Transport Security) នៅលើ Production
    app.UseHsts();
}

app.UseHttpsRedirection();

// អនុញ្ញាតឱ្យប្រព័ន្ធទាញយកឯកសារ Static Files (CSS, Images, JS) ពី Folder wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Map ផ្លូវរត់ (Routing) ទៅកាន់ Razor Pages ទាំងអស់នៅក្នុង Folder Pages (ដោះស្រាយបញ្ហា Error 404 ផ្ដាច់ឫស)
app.MapRazorPages();

// ==========================================
// 4. RUN APPLICATION
// ==========================================

app.Run();