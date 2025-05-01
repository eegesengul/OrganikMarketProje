using Microsoft.AspNetCore.Mvc;
using OrganikMarketProje.Models;
using OrganikMarketProje.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace OrganikMarketProje.Controllers
{
    public class RecipeController : Controller
    {
        private readonly AppDbContext _context;

        public RecipeController(AppDbContext context)
        {
            _context = context;
        }

        // Tarifleri listele
        public IActionResult Index()
        {
            var recipes = _context.Recipes
                .Include(r => r.Ingredients!)
                .ThenInclude(ri => ri.Product)
                .ToList();

            return View(recipes);
        }

        // Tarif ekleme formu
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Products = _context.Products.ToList();  // Ürünleri View'a gönder
            return View();
        }

        // Tarif ekleme işlemi
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Recipe recipe, List<int> SelectedProductIds, List<string> Quantities, IFormFile image)
        {
            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                recipe.ImageData = ms.ToArray();
                recipe.ImageType = image.ContentType;
            }

            recipe.Ingredients = new List<RecipeIngredient>();

            for (int i = 0; i < SelectedProductIds.Count; i++)
            {
                // Eğer Quantities[i] geçerli bir sayıya dönüştürülebiliyorsa ve sıfırdan büyükse
                if (i < Quantities.Count && int.TryParse(Quantities[i], out int quantity) && quantity > 0)
                {
                    recipe.Ingredients.Add(new RecipeIngredient
                    {
                        ProductId = SelectedProductIds[i],
                        Quantity = quantity // Burada `Quantity`'yi integer olarak saklıyoruz
                    });
                }
                else
                {
                    // Eğer geçersiz miktar girilmişse, ModelState'e hata ekleniyor
                    ModelState.AddModelError("", $"Geçersiz miktar: {Quantities[i]}");
                }
            }

            // ModelState geçerli değilse formu tekrar göster
            if (!ModelState.IsValid)
            {
                ViewBag.Products = _context.Products.ToList();
                return View(recipe);
            }

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        // Tarif detayları
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
