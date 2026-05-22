using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhumKasikam.Data;   
using PhumKasikam.Models; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PhumKasikam.Pages.Admin
{
    [Authorize]
    // 🔒 ថែមបន្ទាត់នេះ ដើម្បីបំផ្លាញ Cache ចោល ការពារមិនឱ្យចុចប៊ូតុង Back បកក្រោយមកមើល Dashboard វិញបាន
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // ប្រកាស Properties សម្រាប់ប្រើនៅលើទំព័រ UI (.cshtml)
        public int TotalCrops { get; set; }
        public int TotalBlogs { get; set; }
        public int TotalProducts { get; set; }
        
        public List<Crop> RecentCrops { get; set; } = new List<Crop>();

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            // ១. រាប់ចំនួនដំណាំពី Database
            TotalCrops = await _context.Crops.CountAsync();

            // ២. ទាញយកដំណាំចុងក្រោយគេចំនួន ៥ ដើម្បីបង្ហាញក្នុងតារាង
            RecentCrops = await _context.Crops
                                        .OrderByDescending(c => c.Id) // តម្រៀបពីថ្មីមកចាស់
                                        .Take(5)                      // យកតែ ៥ ដំណាំទេ
                                        .ToListAsync();

            // 🟡 បិទចោលសិន ដើម្បីកុំឱ្យទាក់ Error CS1061 ត្រង់ Blogs ពេលយើងមិនទាន់បាន Migration
            TotalBlogs = 0; 
            TotalProducts = 0;
        }

        // 🚪 មុខងារសម្រាប់ដោះស្រាយការចុចប៊ូតុង Logout ពីគ្រប់ទំព័រ (តាមរយៈ _AdminLayout)
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            // ១. ជម្រះចោលនូវ Cookie Authentication របស់ User ពីក្នុងប្រព័ន្ធ
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // ២. រុញត្រឡប់ទៅកាន់ទំព័រ Login វិញភ្លាមៗ
            return RedirectToPage("/Admin/Login");
        }
    }
}