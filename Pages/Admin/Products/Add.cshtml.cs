using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhumKasikam.Data; // 🟢 ពិនិត្យ Namespace Data របស់អ្នក
using PhumKasikam.Models; // 🟢 ពិនិត្យ Namespace Models របស់អ្នក

namespace PhumKasikam.Pages.Admin.Products
{
    public class AddModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AddModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Product Product { get; set; } = new Product();

        [BindProperty]
        public IFormFile? UploadedImage { get; set; } // សម្រាប់ទទួលហ្វាយរូបភាពពី Form

        public void OnGet()
        {
            // បើកបង្ហាញទំព័រ Form ដំបូង
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 🛠️ Logic គ្រប់គ្រងការ Upload រូបភាព
            if (UploadedImage != null)
            {
                // បង្កើត Folder 'uploads' ក្នុង wwwroot បើមិនទាន់មាន
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // បង្កើតឈ្មោះហ្វាយថ្មីមិនឱ្យជាន់គ្នា (GUID)
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(UploadedImage.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // រក្សាទុកហ្វាយរូបភាពទៅក្នុង Server
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadedImage.CopyToAsync(fileStream);
                }

                // រក្សាទុកផ្លូវរូបភាពទៅក្នុង Database (ឧទាហរណ៍៖ /uploads/abc.jpg)
                Product.ImageUrl = "/uploads/" + uniqueFileName;
            }

            // រក្សាទុកទិន្នន័យផលិតផលចូល SQL Database
            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            // បន្ទាប់ពីរក្សាទុកជោគជ័យ បញ្ជូន Admin ទៅកាន់ទំព័រទីផ្សារ ឬបញ្ជីទំនិញ
            return RedirectToPage("/Market/market");
        }
    }
}