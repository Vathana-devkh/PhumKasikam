using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using PhumKasikam.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhumKasikam.Pages.Blog
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PhumKasikam.Models.Blog? BlogItem { get; set; }
        
        // 🟢 ថែម៖ បញ្ជីអត្ថបទផ្សេងទៀតសម្រាប់អាន
        public List<PhumKasikam.Models.Blog> RelatedBlogs { get; set; } = new List<PhumKasikam.Models.Blog>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // ១. ទាញយកអត្ថបទបច្ចុប្បន្នដែលអ្នកប្រើប្រាស់ចុចមើល
            BlogItem = await _context.Blogs.FirstOrDefaultAsync(b => b.Id == id);

            if (BlogItem == null)
            {
                return Page(); // បើរកមិនឃើញ នឹងបង្ហាញផ្ទាំង "រកមិនឃើញអត្ថបទ"
            }

            // 🟢 ២. ទាញយកអត្ថបទ ៣ ផ្សេងទៀត (ដោយមិនយកអត្ថបទបច្ចុប្បន្ន [b.Id != id])
            RelatedBlogs = await _context.Blogs
                .Where(b => b.Id != id) 
                .OrderByDescending(b => b.CreatedAt)
                .Take(3)
                .ToListAsync();

            return Page();
        }
    }
}