using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganikMarketProje.Data;
using OrganikMarketProje.Models;
using OrganikMarketProje.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganikMarketProje.Controllers
{
    public class RecipeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CartService _cartService;
        private readonly UserManager<AppUser> _userManager;

        public RecipeController(AppDbContext context, CartService cartService, UserManager<AppUser> userManager)
        {
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            return RedirectToAction("Index", new { query });
        }

        public IActionResult Index(string? query)
        {
            ViewBag.ShowRecipeSearch = true;

            var recipes = _context.Recipes
                .Include(r => r.Ingredients!)
                    .ThenInclude(ri => ri.Product)
                .ToList();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                recipes = recipes
                    .Where(r =>
                        r.Title.ToLower().Contains(query) ||
                        r.Instructions.ToLower().Contains(query)
                    ).ToList();

                if (!recipes.Any())
                {
                    TempData["RecipeSearchInfo"] = "Aradığınız kriterlere uygun tarif bulunamadı.";
                }
            }

            var cartItems = _cartService.GetCart();
            var cartProductIds = cartItems.Select(c => c.ProductId).ToList();

            var matchingRecipes = recipes
                .Where(r => r.Ingredients != null && r.Ingredients.Any(ri => cartProductIds.Contains(ri.ProductId)))
                .ToList();

            ViewBag.SuggestedRecipes = matchingRecipes;

            return View(recipes);
        }

        public IActionResult Details(int id)
        {
            var recipe = _context.Recipes
                .Include(r => r.Ingredients!)
                    .ThenInclude(ri => ri.Product)
                .FirstOrDefault(r => r.Id == id);

            if (recipe == null)
                return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.IsFavorite = _context.FavoriteRecipes.Any(f => f.RecipeId == id && f.UserId == userId);
            }

            var comments = _context.Comments
                .Include(c => c.User)
                .Where(c => c.RecipeId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.RecipeComments = comments;
            ViewBag.AverageRating = comments.Any() ? comments.Average(c => c.Rating) : 0;
            ViewBag.RecipeId = id;

            return View(recipe);
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

            recipe.Ingredients = IngredientForms
                .Where(i => i.IsSelected && i.Quantity > 0)
                .Select(i => new RecipeIngredient
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList();

            if (!recipe.Ingredients.Any())
            {
                ModelState.AddModelError("", "En az bir malzeme seçmelisiniz.");
                ViewBag.Products = _context.Products.ToList();
                return View(recipe);
            }

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
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

            _context.RecipeIngredients.RemoveRange(recipe.Ingredients);

            recipe.Ingredients = IngredientForms
                .Where(i => i.IsSelected && i.Quantity > 0)
                .Select(i => new RecipeIngredient
                {
                    ProductId = i.ProductId,
                    RecipeId = recipe.Id,
                    Quantity = i.Quantity
                }).ToList();

            await _context.SaveChangesAsync();

            TempData["RecipeMessage"] = "Tarif başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
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

            try
            {
                // İlişkili yorumlar
                var comments = _context.Comments.Where(c => c.RecipeId == id);
                _context.Comments.RemoveRange(comments);

                // Favori tarifler
                var favorites = _context.FavoriteRecipes.Where(f => f.RecipeId == id);
                _context.FavoriteRecipes.RemoveRange(favorites);

                // Malzemeler
                _context.RecipeIngredients.RemoveRange(recipe.Ingredients);

                // Tarif
                _context.Recipes.Remove(recipe);

                _context.SaveChanges();

                TempData["RecipeMessage"] = "Tarif başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["RecipeMessage"] = $"Tarif silinirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
