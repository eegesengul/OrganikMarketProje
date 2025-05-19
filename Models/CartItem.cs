using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class CartItem
    {
        [Key]  // 🔑 Primary key tanımı
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public AppUser? User { get; set; }
    }
}
