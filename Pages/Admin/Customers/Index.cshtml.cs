using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using PhumKasikam.Models; // 🟢 ត្រូវប្រាកដថា CustomerOrder ឬ Model របស់អ្នកស្ថិតក្នុង Namespace នេះ

namespace PhumKasikam.Pages.Admin.Customers
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // បង្កើតបញ្ជីសម្រាប់ផ្ទុកទិន្នន័យទៅបង្ហាញលើ HTML Table
        public IList<CustomerOrder> CustomerOrders { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.CustomerOrders != null)
            {
                // ទាញយកទិន្នន័យការកម្ម៉ង់ ដោយតម្រៀបពីថ្មីបំផុតទៅចាស់បំផុត (Latest First)
                CustomerOrders = await _context.CustomerOrders
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
        }

        // 🟢 Handler សម្រាប់ឱ្យ Admin ចុចប្តូរស្ថានភាពការកម្ម៉ង់ (ឧ. ពី Pending ទៅ Approved)
        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
        {
            var order = await _context.CustomerOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}