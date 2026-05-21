using System;
using System.ComponentModel.DataAnnotations;

namespace PhumKasikam.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "សូមបំពេញចំណងជើងអត្ថបទ")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "សូមបំពេញខ្លឹមសារអត្ថបទ")]
        public string Content { get; set; } = string.Empty;

        // 🟢 ត្រូវប្រាកដថាមានសញ្ញាសួរ (?) នេះ ដើម្បីប្រាប់ប្រព័ន្ធថាវាអាចផ្ទុកតម្លៃ Null បាននៅពេលដំបូង
        public string? ImageUrl { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}