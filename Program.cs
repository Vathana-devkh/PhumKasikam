using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. ទាញយក Connection String ពី appsettings.json (ឬពី Environment Variables លើ Cloud)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. កែប្រែពី UseMySql មកប្រើ UseNpgsql សម្រាប់ PostgreSQL វិញ 💥
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// អនុញ្ញាតឱ្យប្រព័ន្ធបង្ហាញឯកសារ Static ដូចជា រូបភាព, CSS, និង JS
app.UseStaticFiles(); 

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// 3. កែតម្រូវកន្លែង Auto-Migration ឱ្យដំណើរការបានត្រឹមត្រូវ (ដំឡើងតារាងទៅ Cloud ស្វ័យប្រវត្តិ) 🛠️
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // ប្រើប្រាស់ .GetAwaiter().GetResult() ព្រោះនៅក្នុង Program.cs មិនមែនជា async method ឡើយ
    dbContext.Database.Migrate(); 
}

app.Run();