using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;    // 🟢 ផ្ទៀងផ្ទាត់ផ្លូវ Namespace Data របស់អ្នក
using PhumKasikam.Models;  // 🟢 ផ្ទៀងផ្ទាត់ផ្លូវ Namespace Models របស់អ្នក

namespace PhumKasikam.Pages.Admin.Products
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

        public IList<Product> ProductsList { get; set; } = default!;

        [BindProperty]
        public Product NewProduct { get; set; } = new Product();

        [BindProperty]
        public Product EditProduct { get; set; } = new Product();

        public async Task OnGetAsync()
        {
            if (_context.Products != null)
            {
                ProductsList = await _context.Products.ToListAsync();
            }
        }

        // 1️⃣ HANDLER: បង្កើតផលិតផលថ្មី
        public async Task<IActionResult> OnPostCreateProductAsync(IFormFile? UploadImage, string? ImageUrlLink)
        {
            if (UploadImage != null)
            {
                NewProduct.ImageUrl = await SaveUploadedFileAsync(UploadImage);
            }
            else if (!string.IsNullOrEmpty(ImageUrlLink))
            {
                NewProduct.ImageUrl = ImageUrlLink;
            }

            _context.Products.Add(NewProduct);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // 2️⃣ HANDLER: កែប្រែព័ត៌មានផលិតផល
        public async Task<IActionResult> OnPostEditProductAsync(IFormFile? EditUploadImage, string? EditImageUrlLink)
        {
            var productToUpdate = await _context.Products.FindAsync(EditProduct.Id);
            if (productToUpdate == null) return NotFound();

            productToUpdate.Name = EditProduct.Name;
            productToUpdate.Category = EditProduct.Category;
            productToUpdate.Price = EditProduct.Price;
            productToUpdate.OriginalPrice = EditProduct.OriginalPrice;
            productToUpdate.Unit = EditProduct.Unit;
            productToUpdate.PriceType = EditProduct.PriceType;
            productToUpdate.Description = EditProduct.Description;

            if (EditUploadImage != null)
            {
                productToUpdate.ImageUrl = await SaveUploadedFileAsync(EditUploadImage);
            }
            else if (!string.IsNullOrEmpty(EditImageUrlLink))
            {
                productToUpdate.ImageUrl = EditImageUrlLink;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        // 3️⃣ HANDLER: លុបផលិតផល
        public async Task<IActionResult> OnPostDeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        // 🛠️ Helper Method សម្រាប់រក្សាទុកហ្វាយរូបភាពចូល wwwroot/uploads
        private async Task<string> SaveUploadedFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return "/uploads/" + uniqueFileName;
        }
    }
}