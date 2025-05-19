using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganikMarketProje.Data;
using OrganikMarketProje.Models;
using OrganikMarketProje.Services;
using System.Linq;
using System.Threading.Tasks;

namespace OrganikMarketProje.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductOperations _productOps;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProductController(ProductOperations productOps, AppDbContext context, UserManager<AppUser> userManager)
        {
            _productOps = productOps;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ViewBag.ShowProductSearch = true;
            var products = _productOps.GetAll();
            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _productOps.GetById(id);
            if (product == null)
                return NotFound();

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.IsFavorite = _context.FavoriteProducts.Any(f => f.ProductId == id && f.UserId == userId);
            }

            var comments = _context.Comments
                .Include(c => c.User)
                .Where(c => c.ProductId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            ViewBag.ProductComments = comments;
            ViewBag.AverageRating = comments.Any() ? comments.Average(c => c.Rating) : 0;
            ViewBag.ProductId = id;

            return View(product);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            ViewBag.ShowProductSearch = true;
            ViewBag.SearchQuery = query;

            var results = _context.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .ToList();

            if (!results.Any())
            {
                TempData["ProductSearchInfo"] = "Aradığınız kriterlere uygun ürün bulunamadı.";
            }

            return View("Index", results);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product, IFormFile image)
        {
            if (!ModelState.IsValid)
                return View(product);

            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                product.ImageData = ms.ToArray();
                product.ImageType = image.ContentType;
            }

            _productOps.Add(product);
            TempData["CartMessage"] = "Ürün başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _productOps.GetById(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Product updatedProduct, IFormFile image)
        {
            if (!ModelState.IsValid)
                return View(updatedProduct);

            var product = _productOps.GetById(id);
            if (product == null)
                return NotFound();

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Category = updatedProduct.Category;
            product.DeliveryInfo = updatedProduct.DeliveryInfo;
            product.StockQuantity = updatedProduct.StockQuantity;

            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                product.ImageData = ms.ToArray();
                product.ImageType = image.ContentType;
            }

            _productOps.Update(product);
            TempData["CartMessage"] = "Ürün başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            try
            {
                _productOps.Delete(id);
                TempData["CartMessage"] = "Ürün başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["CartError"] = $"Silme işlemi sırasında hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
