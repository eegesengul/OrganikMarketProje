using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [Required]
        public int StockQuantity { get; set; }


        public string DeliveryInfo { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }
}
