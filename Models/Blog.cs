using System; // សម្រាប់ DateTime
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhumKasikam.Models // ត្រូវមាន Namespace នេះ
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public string? ImageUrl { get; set; }
        
        [Column(TypeName = "timestamp")] 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}