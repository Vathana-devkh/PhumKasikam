using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhumKasikam.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal Price { get; set; } // តម្លៃលក់បច្ចុប្បន្ន
        public decimal? OriginalPrice { get; set; } // 🟢 តម្លៃដើមមុនបញ្ចុះតម្លៃ (អាចទំនេរបាន)
        public string? PriceType { get; set; }
        public string? Unit { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}