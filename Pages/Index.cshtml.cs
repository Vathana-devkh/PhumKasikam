using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhumKasikam.Data;
using PhumKasikam.Models;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Crop> Crops { get; set; } = new();

    public async Task OnGetAsync()
    {
        Crops = await _context.Crops.ToListAsync();
    }
}