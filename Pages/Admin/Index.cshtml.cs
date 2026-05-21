using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhumKasikam.Data;   // 🟢 ត្រូវប្រាកដថាស្គាល់ Folder Data របស់ ApplicationDbContext
using PhumKasikam.Models; // 🟢 ត្រូវប្រាកដថាស្គាល់ Folder Models របស់ Crop

namespace PhumKasikam.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // ប្រកាស Properties សម្រាប់ប្រើនៅលើទំព័រ UI (.cshtml)
        public int TotalCrops { get; set; }
        public int TotalBlogs { get; set; }
        public int TotalProducts { get; set; }
        
        // 🟢 បន្ថែម Property នេះដើម្បីដោះស្រាយ Error ត្រង់ RecentCrops
        public List<Crop> RecentCrops { get; set; } = new List<Crop>();

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
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
    }
}