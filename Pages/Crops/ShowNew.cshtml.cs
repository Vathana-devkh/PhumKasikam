using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using PhumKasikam.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhumKasikam.Pages.Crops
{
    public class ShowNewModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ShowNewModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PhumKasikam.Models.Crop> CropsList { get; set; } = new List<PhumKasikam.Models.Crop>();
        
        // 🟢 ថែម៖ សម្រាប់ផ្ទុកប្រភេទដំណាំយកទៅធ្វើជា Tabs Filter
        public List<string> Categories { get; set; } = new List<string>();
        
        // 🟢 ថែម៖ សម្រាប់ចំណាំថាគេកំពុងមើល Category មួយណា (Default គឺ "ទាំងអស់")
        public string SelectedCategory { get; set; } = "ទាំងអស់";

        public async Task<IActionResult> OnGetAsync(string? category)
        {
            // ១. ទាញយកឈ្មោះប្រភេទដំណាំប្លែកៗពីគ្នាដែលមាននៅក្នុង DB មកធ្វើជាប៊ូតុង Filter
            // (ប្រសិនបើលោកអ្នកមានតារាង Category ដាច់ដោយឡែក អាចប្តូរទៅទាញពីតារាងនោះបាន)
            Categories = await _context.Crops
                .Where(c => c.CategoryId != 0) // ការពារទិន្នន័យគ្មានប្រភេទ
                .Select(c => c.CategoryId == 1 ? "បន្លែសរីរាង្គ" : 
                             c.CategoryId == 2 ? "ផ្លែឈើដាំដុះ" : 
                             c.CategoryId == 3 ? "គ្រាប់ពូជកសិកម្ម" : "ផ្សេងៗ")
                .Distinct()
                .ToListAsync();

            // ២. ចាប់យកលក្ខខណ្ឌ Filter ពី URL Query String (?category=...)
            if (!string.IsNullOrEmpty(category))
            {
                SelectedCategory = category;
            }

            // ៣. ទាញយកទិន្នន័យដំណាំមកបង្ហាញតាមលក្ខខណ្ឌ Filter
            var query = _context.Crops.AsQueryable();

            if (SelectedCategory != "ទាំងអស់")
            {
                int targetId = SelectedCategory == "បន្លែសរីរាង្គ" ? 1 :
                               SelectedCategory == "ផ្លែឈើដាំដុះ" ? 2 :
                               SelectedCategory == "គ្រាប់ពូជកសិកម្ម" ? 3 : 0;

                query = query.Where(c => c.CategoryId == targetId);
            }

            CropsList = await query.OrderByDescending(c => c.Id).ToListAsync();

            return Page();
        }
    }
}