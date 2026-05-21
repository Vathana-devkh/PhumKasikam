using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using System.Threading.Tasks;

namespace PhumKasikam.Pages.Crops
{
    public class DetailsNewModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsNewModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PhumKasikam.Models.Crop? CropItem { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // ទាញយកទិន្នន័យដំណាំតែមួយគត់ដោយផ្អែកលើ id ដែលបានចុចផ្ទេរមក
            CropItem = await _context.Crops.FirstOrDefaultAsync(c => c.Id == id);

            if (CropItem == null)
            {
                return Page(); // បើរកមិនឃើញ វានឹងបង្ហាញផ្ទាំង Error អត់មានទិន្នន័យជំនួសវិញ
            }

            return Page();
        }
    }
}