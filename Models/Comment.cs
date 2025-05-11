using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
