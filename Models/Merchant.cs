namespace PhumKasikam.Models
{
    public class Merchant
    {
        public int Id { get; set; }
        public string Name { get; set; } = ""; // ឈ្មោះឈ្មួញ
        public string BuyingProduct { get; set; } = ""; // កសិផលដែលទិញ
        public string? Location { get; set; } // ថែមជួរនេះចូល
        public string PhoneNumber { get; set; } = ""; // លេខទូរស័ព្ទ
    }
}