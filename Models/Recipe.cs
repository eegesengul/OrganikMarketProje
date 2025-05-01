using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty; // Tarif Başlığı

        [Required]
        public string Instructions { get; set; } = string.Empty; // Tarif Açıklaması (Nasıl Yapılır?)

        [Required]
        public int DurationMinutes { get; set; } // Tarif Süresi (Dakika)

        public byte[]? ImageData { get; set; } // Tarif Fotoğrafı
        public string? ImageType { get; set; }

        // İleride tarifin malzeme listesi olacak (Many-to-Many ilişki)
        public ICollection<RecipeIngredient>? Ingredients { get; set; }
    }
}
