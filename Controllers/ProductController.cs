using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganikMarketProje.Data;
using OrganikMarketProje.Models;
using OrganikMarketProje.Services;
using System.Linq;

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
                var isFav = _context.FavoriteProducts.Any(f => f.ProductId == id && f.UserId == userId);
                ViewBag.IsFavorite = isFav;
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

            return View("Index", results); // mevcut ürün listeleme görünümünü kullan
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
            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                product.ImageData = ms.ToArray();
                product.ImageType = image.ContentType;
            }

            _productOps.Add(product);
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
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            _productOps.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
    