using Microsoft.AspNetCore.Mvc;
using OrganikMarketProje.Models;
using OrganikMarketProje.Data;
using OrganikMarketProje.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace OrganikMarketProje.Controllers
{
    public class RecipeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CartService _cartService;

        public RecipeController(AppDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var recipes = _context.Recipes
                .Include(r => r.Ingredients!)
                .ThenInclude(ri => ri.Product)
                .ToList();

            var cartItems = _cartService.GetCart();
            var cartProductIds = cartItems.Select(c => c.ProductId).ToList();

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

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var recipe = _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null)
                return NotFound();

            ViewBag.Products = _context.Products.ToList();
            return View(recipe);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Recipe updatedRecipe, List<IngredientFormModel> IngredientForms, IFormFile image)
        {
            var recipe = _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null)
                return NotFound();

            recipe.Title = updatedRecipe.Title;
            recipe.Instructions = updatedRecipe.Instructions;
            recipe.DurationMinutes = updatedRecipe.DurationMinutes;

            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                recipe.ImageData = ms.ToArray();
                recipe.ImageType = image.ContentType;
            }

            var existingIngredients = _context.RecipeIngredients.Where(ri => ri.RecipeId == recipe.Id);
            _context.RecipeIngredients.RemoveRange(existingIngredients);
            await _context.SaveChangesAsync();

            var newIngredients = new List<RecipeIngredient>();
            foreach (var item in IngredientForms)
            {
                if (item.IsSelected && item.Quantity > 0)
                {
                    newIngredients.Add(new RecipeIngredient
                    {
                        RecipeId = recipe.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
                }
            }

            if (newIngredients.Count > 0)
            {
                await _context.RecipeIngredients.AddRangeAsync(newIngredients);
            }

            await _context.SaveChangesAsync();

            TempData["RecipeMessage"] = "Tarif başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var recipe = _context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null)
            {
                TempData["RecipeMessage"] = "Silmek istediğiniz tarif bulunamadı.";
                return RedirectToAction("Index");
            }

            if (recipe.Ingredients != null)
            {
                _context.RecipeIngredients.RemoveRange(recipe.Ingredients);
            }

            _context.Recipes.Remove(recipe);
            _context.SaveChanges();

            TempData["RecipeMessage"] = "Tarif başarıyla silindi.";
            return RedirectToAction("Index");
        }
    }
}
