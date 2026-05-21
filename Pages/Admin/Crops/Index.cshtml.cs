using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using PhumKasikam.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhumKasikam.Pages.Admin.Crops
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<PhumKasikam.Models.Crop> CropsList { get; set; } = new List<PhumKasikam.Models.Crop>();

        [BindProperty]
        public PhumKasikam.Models.Crop NewCrop { get; set; } = new PhumKasikam.Models.Crop();

        [BindProperty]
        public PhumKasikam.Models.Crop EditCrop { get; set; } = new PhumKasikam.Models.Crop();

        public async Task OnGetAsync()
        {
            CropsList = await _context.Crops.OrderByDescending(c => c.Id).ToListAsync();
        }

        // ================= 02. CREATE ACTION =================
        public async Task<IActionResult> OnPostCreateCropAsync(IFormFile? UploadImage, string? ImageUrlLink)
        {
            // ១. ពិនិត្យមើលបើមានការបង្ហោះហ្វាយរូបភាពផ្ទាល់ពីម៉ាស៊ីន
            if (UploadImage != null && UploadImage.Length > 0)
            {
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(UploadImage.FileName);
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "crops");
                
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadImage.CopyToAsync(fileStream);
                }
                NewCrop.ImageUrl = "/images/crops/" + uniqueFileName;
            }
            // ២. បើគ្មានហ្វាយទេ ប៉ុន្តែមានការដាក់ Link (Copy Image Address)
            else if (!string.IsNullOrEmpty(ImageUrlLink))
            {
                NewCrop.ImageUrl = ImageUrlLink;
            }
            // ៣. បើគ្មានទាំងពីរ ដាក់រូបភាពលំនាំដើម (Placeholder)
            else
            {
                NewCrop.ImageUrl = "https://placehold.co/400x300";
            }

            _context.Crops.Add(NewCrop);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // ================= 03. UPDATE ACTION =================
        public async Task<IActionResult> OnPostEditCropAsync(IFormFile? EditUploadImage, string? EditImageUrlLink)
        {
            var cropInDb = await _context.Crops.FindAsync(EditCrop.Id);
            if (cropInDb == null) return NotFound();

            cropInDb.Name = EditCrop.Name;
            cropInDb.Description = EditCrop.Description;
            cropInDb.CategoryId = EditCrop.CategoryId;

            // ១. បើមានការប្ដូរដោយបង្ហោះហ្វាយរូបភាពថ្មី
            if (EditUploadImage != null && EditUploadImage.Length > 0)
            {
                // លុបរូបភាពចាស់ពីម៉ាស៊ីន (បើជារូបភាពក្នុង Server)
                if (!string.IsNullOrEmpty(cropInDb.ImageUrl) && cropInDb.ImageUrl.StartsWith("/"))
                {
                    string oldFilePath = Path.Combine(_environment.WebRootPath, cropInDb.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(EditUploadImage.FileName);
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "crops");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await EditUploadImage.CopyToAsync(fileStream);
                }
                cropInDb.ImageUrl = "/images/crops/" + uniqueFileName;
            }
            // ២. បើគេដូរដោយការផាស Link (Copy Image Address) ថ្មីចូល
            else if (!string.IsNullOrEmpty(EditImageUrlLink))
            {
                cropInDb.ImageUrl = EditImageUrlLink;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // ================= 04. DELETE ACTION =================
        public async Task<IActionResult> OnPostDeleteCropAsync(int id)
        {
            var crop = await _context.Crops.FindAsync(id);
            if (crop == null) return NotFound();

            if (!string.IsNullOrEmpty(crop.ImageUrl) && crop.ImageUrl.StartsWith("/"))
            {
                var filePath = Path.Combine(_environment.WebRootPath, crop.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            }

            _context.Crops.Remove(crop);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}