using System;
using System.ComponentModel.DataAnnotations;

namespace PhumKasikam.Models
{
    public class CustomerOrder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Region { get; set; } = string.Empty; // phnompenh ឬ province

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string DeliveryService { get; set; } = string.Empty;

        [StringLength(100)]
        public string? BranchName { get; set; }

        [Required]
        public string PayslipPath { get; set; } = string.Empty;

        [Required]
        public string CartItems { get; set; } = string.Empty; // រក្សាទុកបញ្ជីទំនិញជា JSON String

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    }
}