using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;    
using PhumKasikam.Models;  
using System.Text.Json;

namespace PhumKasikam.Pages.Market
{
    public class MarketModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MarketModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IList<Product> Products { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Products != null)
            {
                Products = await _context.Products.ToListAsync();
            }
        }

        // 🟢 ដំណោះស្រាយ៖ កែសម្រួលឈ្មោះ Parameter ឱ្យទៅជា PascalCase ដូច JavaScript FormData និងដាក់ [FromForm] ឱ្យចំឈ្មោះ PayslipFile
        public async Task<IActionResult> OnPostSubmitOrderAsync(
            [FromForm] string CustomerName,
            [FromForm] string Phone,
            [FromForm] string Email,
            [FromForm] string Region,
            [FromForm] string Location,
            [FromForm] string DeliveryService,
            [FromForm] string BranchName,
            [FromForm] string CartItemsJson,
            [FromForm] IFormFile PayslipFile)
        {
            try
            {
                // 1. ពិនិត្យលក្ខខណ្ឌទិន្នន័យចាំបាច់ (Validation)
                if (string.IsNullOrEmpty(CustomerName) || string.IsNullOrEmpty(Phone) || string.IsNullOrEmpty(Location))
                {
                    return new JsonResult(new { success = false, message = "សូមបំពេញព័ត៌មានដែលចាំបាច់ឱ្យបានគ្រប់ជ្រុងជ្រោយ (*)" });
                }

                if (PayslipFile == null || PayslipFile.Length == 0)
                {
                    return new JsonResult(new { success = false, message = "សូមភ្ជាប់មកជាមួយនូវរូបភាពវិក្កយបត្របង់ប្រាក់ (Payslip)" });
                }

                // 2. ដំណើរការរក្សាទុករូបភាពចុងសន្លឹកបង់ប្រាក់ (Upload Payslip)
                string payslipUrl = "";
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "payslips");
                
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // បង្កើតឈ្មោះ File ថ្មីកុំឱ្យជាន់គ្នា (Guid)
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PayslipFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PayslipFile.CopyToAsync(fileStream);
                }
                
                // ផ្លូវទឹក (URL Path) សម្រាប់យកទៅរក្សាទុកក្នុង Database
                payslipUrl = "/uploads/payslips/" + uniqueFileName;

                // 3. ដំណើរការរក្សាទុកទិន្នន័យកម្ម៉ង់របស់អតិថិជនចូល Database ពិតប្រាកដ
                var newOrder = new CustomerOrder
                {
                    Name = CustomerName,
                    Phone = Phone,
                    Email = Email,
                    Region = Region,
                    Location = Location,
                    DeliveryService = DeliveryService,
                    BranchName = BranchName,
                    PayslipPath = payslipUrl,
                    CartItems = CartItemsJson, 
                    OrderDate = DateTime.Now,
                    Status = "Pending" 
                };

                _context.CustomerOrders.Add(newOrder);
                
                // 🟢 បន្ទាត់ស្នូល៖ រុញនិងរក្សាទុកបម្រែបម្រួលចូលទៅក្នុង SQL Server Database
                await _context.SaveChangesAsync();
            
                // ប្រសិនបើដំណើរការរក្សាទុកជោគជ័យ ផ្ញើសារត្រឡប់ទៅឱ្យ JavaScript វិញ
                return new JsonResult(new { success = true, message = "ការកម្ម៉ង់របស់អ្នកទទួលបានជោគជ័យ និងត្រូវបានបញ្ជូនទៅផ្នែកគ្រប់គ្រង!" });
            }
            catch (Exception ex)
            {
                // ចាប់យកកំហុសប្រព័ន្ធ (Exception) រួចបោះសារលម្អិតទៅឱ្យ Frontend មើលក្នុងករណីមានបញ្ហា Database
                var baseException = ex.GetBaseException();
                return new JsonResult(new { success = false, message = "កំហុសម៉ាស៊ីនបម្រើ៖ " + baseException.Message });
            }
        }
    }
}