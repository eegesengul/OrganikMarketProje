using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        public decimal Price { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [Required(ErrorMessage = "Stok adedi zorunludur.")]
        public int StockQuantity { get; set; }

        public string DeliveryInfo { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
