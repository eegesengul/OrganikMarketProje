using Microsoft.AspNetCore.Mvc;
using OrganikMarketProje.Models;
using OrganikMarketProje.Data;
using OrganikMarketProje.Services; // EKLEDİK
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace OrganikMarketProje.Controllers
{
    public class RecipeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CartService _cartService; // EKLEDİK

        public RecipeController(AppDbContext context, CartService cartService) // DIJEKSIYON EKLEDİK
        {
            _context = context;
            _cartService = cartService;
        }

        // Tarifleri listele
        public IActionResult Index()
        {
            var recipes = _context.Recipes
                .Include(r => r.Ingredients!)
                .ThenInclude(ri => ri.Product)
                .ToList();

            var cartItems = _cartService.GetCart();

            // Kullanıcının sepetindeki ürün id'lerini al
            var cartProductIds = cartItems.Select(c => c.ProductId).ToList();

            // Sepetteki ürünlerle yapılabilecek tarifleri bul
            var matchingRecipes = recipes
                .Where(r => r.Ingredients != null && r.Ingredients.Any(ri => cartProductIds.Contains(ri.ProductId)))
                .ToList();

            ViewBag.SuggestedRecipes = matchingRecipes;

            return View(recipes);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Recipe recipe, List<IngredientFormModel> IngredientForms, IFormFile image)
        {
            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                recipe.ImageData = ms.ToArray();
                recipe.ImageType = image.ContentType;
            }

            recipe.Ingredients = new List<RecipeIngredient>();

            foreach (var item in IngredientForms)
            {
                if (item.IsSelected && item.Quantity > 0)
                {
                    recipe.Ingredients.Add(new RecipeIngredient
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
                }
            }

            if (recipe.Ingredients.Count == 0)
            {
                ModelState.AddModelError("", "En az bir malzeme seçmelisiniz.");
                ViewBag.Products = _context.Products.ToList();
                return View(recipe);
            }

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var recipe = _context.Recipes
                .Include(r => r.Ingredients!)
                .ThenInclude(ri => ri.Product)
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }
    }
}
