using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PhumKasikam.Pages.Admin
{
    [Authorize] // 🔒 ការពារទំព័រ Dashboard នេះមិនឱ្យអ្នកក្រៅចូលមើលដាច់ខាត
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // 📊 បណ្តា Properties សម្រាប់បោះទៅឱ្យ UI (@Model.TotalCrops, ...)
        public int TotalCrops { get; set; }
        public int TotalBlogs { get; set; }
        public int TotalProducts { get; set; }
        public List<CropViewModel> RecentCrops { get; set; } = new List<CropViewModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Admin/Login");
            }

            // ២. កំណត់ Header ដើម្បីកុំឱ្យ Browser ទុក Cache
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            // 1. រាប់ចំនួនទិន្នន័យសរុបពី Database ពិតប្រាកដ
            TotalCrops = await _context.Crops.CountAsync();
            TotalBlogs = await _context.Blogs.CountAsync();
            TotalProducts = await _context.Products.CountAsync();

            // 2. ទាញយកទិន្នន័យដំណាំដែលទើបតែបញ្ចូលថ្មីៗបំផុតចំនួន ៥ (ផ្អែកលើ Id ឬ CreatedAt លំដាប់ចុះ)
            // 💡 ប្រសិនបើ Model របស់លោកអ្នកមាន CreatedAt អាចដូរទៅជា .OrderByDescending(c => c.CreatedAt) បាន
            RecentCrops = await _context.Crops
                .OrderByDescending(c => c.Id) 
                .Take(5)
                .Select(c => new CropViewModel
                {
                    Name = c.Name,
                    Category = c.Category ?? "ទូទៅ"
                })
                .ToListAsync();
            return Page();
        }

        // 🚪 មុខងារចាកចេញពីប្រព័ន្ធ (Logout)
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Admin/Login");
        }
    }

    // 💡 Class ជំនួយសម្រាប់រ៉ាប់រងការបង្ហាញទិន្នន័យក្នុងតារាង RecentCrops
    public class CropViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}