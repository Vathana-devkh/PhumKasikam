using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using PhumKasikam.Models;

namespace PhumKasikam.Pages.Crops
{
    public class CropsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CropsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Crop> Crops { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // ទាញយកទិន្នន័យទាំងអស់ពី Table Crops
            Crops = await _context.Crops.ToListAsync();
        }
    }
}