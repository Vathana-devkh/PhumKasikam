using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using PhumKasikam.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace PhumKasikam.Pages.Blog
{
    public class BlogModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        // បង្កើត List ឈ្មោះ Blogs ឱ្យត្រូវបេះបិទទៅនឹងការហៅប្រើក្នុងទំព័រ HTML
        public List<PhumKasikam.Models.Blog> Blogs { get; set; } = new List<PhumKasikam.Models.Blog>();

        public BlogModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // ទាញយកទិន្នន័យអត្ថបទទាំងអស់ពី MySQL តម្រៀបពីថ្មីទៅចាស់
            Blogs = await _context.Blogs
                                  .OrderByDescending(b => b.CreatedAt)
                                  .ToListAsync();
        }
    }
}