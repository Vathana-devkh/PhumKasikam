using Microsoft.EntityFrameworkCore;
using PhumKasikam.Models;


namespace PhumKasikam.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Crop> Crops { get; set; } 
        public DbSet<Merchant> Merchants { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
    }
}