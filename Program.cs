using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;

var builder = WebApplication.CreateBuilder(args);

// 1.  Connection String to appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. signature ApplicationDbContext for use  MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

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

// add use for static files for show photo CSS/JS files
app.UseStaticFiles(); 

app.UseRouting();

app.UseAuthorization();


app.MapRazorPages();

app.Run();