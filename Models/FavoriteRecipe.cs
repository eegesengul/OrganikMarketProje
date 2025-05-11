namespace OrganikMarketProje.Models;

public class FavoriteRecipe
{
    public int Id { get; set; }

    public string UserId { get; set; }
    public AppUser User { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }
}
