using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrganikMarketProje.Data;
using OrganikMarketProje.Models;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class FavoriteController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public FavoriteController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> MyFavorites()
    {
        var userId = _userManager.GetUserId(User);

        var favProducts = _context.FavoriteProducts
            .Include(f => f.Product)
            .Where(f => f.UserId == userId)
            .ToList();

        var favRecipes = _context.FavoriteRecipes
            .Include(f => f.Recipe)
            .Where(f => f.UserId == userId)
            .ToList();

        ViewBag.FavProducts = favProducts;
        ViewBag.FavRecipes = favRecipes;

        return View();
    }

    [HttpPost]
    public IActionResult ToggleProduct(int productId)
    {
        var userId = _userManager.GetUserId(User);
        var existing = _context.FavoriteProducts.FirstOrDefault(f => f.ProductId == productId && f.UserId == userId);

        if (existing != null)
            _context.FavoriteProducts.Remove(existing);
        else
            _context.FavoriteProducts.Add(new FavoriteProduct { ProductId = productId, UserId = userId });

        _context.SaveChanges();
        return RedirectToAction("Details", "Product", new { id = productId });
    }

    [HttpPost]
    public IActionResult ToggleRecipe(int recipeId)
    {
        var userId = _userManager.GetUserId(User);
        var existing = _context.FavoriteRecipes.FirstOrDefault(f => f.RecipeId == recipeId && f.UserId == userId);

        if (existing != null)
            _context.FavoriteRecipes.Remove(existing);
        else
            _context.FavoriteRecipes.Add(new FavoriteRecipe { RecipeId = recipeId, UserId = userId });

        _context.SaveChanges();
        return RedirectToAction("Details", "Recipe", new { id = recipeId });
    }
}
