using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hazırlanışı zorunludur.")]
        [StringLength(1000, ErrorMessage = "Hazırlanış açıklaması en fazla 1000 karakter olabilir.")]
        public string Instructions { get; set; } = string.Empty;

        [Required(ErrorMessage = "Süre zorunludur.")]
        public int DurationMinutes { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        public ICollection<RecipeIngredient>? Ingredients { get; set; }
    }
}
