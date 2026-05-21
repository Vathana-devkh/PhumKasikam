using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhumKasikam.Data;
using PhumKasikam.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PhumKasikam.Pages.Admin.Crops
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Crop Crop { get; set; } = default!;

        // ចាប់យក File រូបភាពដែលបាន Upload មកពី Form 
        [BindProperty]
        public IFormFile? UploadImage { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // ដំណើរការរក្សាទុករូបភាព (Image Upload Process)
            if (UploadImage != null)
            {
                // កំណត់ទីតាំង Folder សម្រាប់ទុក៖ wwwroot/images/crops/
                string folderPath = Path.Combine(_environment.WebRootPath, "images", "crops");
                
                // ប្រសិនបើមិនទាន់មាន Folder នេះទេ ឱ្យប្រព័ន្ធបង្កើតដោយស្វ័យប្រវត្តិ
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // បង្កើតឈ្មោះរូបភាពថ្មីមួយដោយប្រើ Guid ដើម្បីកុំឱ្យជាន់គ្នា (ឧ. a1b2c3d4_cashew.jpg)
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(UploadImage.FileName);
                string filePath = Path.Combine(folderPath, uniqueFileName);

                // រក្សាទុក File ចូលទៅក្នុង Host Drive
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadImage.CopyToAsync(fileStream);
                }

                // រក្សាទុកផ្លូវ (Path) នៃរូបភាពទៅក្នុង Database ដើម្បីយកទៅបង្ហាញលើ View
                Crop.ImageUrl = "/images/crops/" + uniqueFileName;
            }
            else
            {
                // ករណីគ្មានរូបភាព អាចដាក់រូបភាព Default ឬទុករួចចាំបញ្ចូលតាមក្រោយ
                Crop.ImageUrl = "/images/crops/default.png";
            }

            // បញ្ចូលទិន្នន័យដំណាំទៅក្នុង Database
            _context.Crops.Add(Crop);
            await _context.SaveChangesAsync();

            // នៅពេលជោគជ័យ ឱ្យវាផ្លាស់ប្តូរទំព័រទៅកាន់ទំព័របញ្ជីដំណាំវិញ
            return RedirectToPage("./Index");
        }
    }
}