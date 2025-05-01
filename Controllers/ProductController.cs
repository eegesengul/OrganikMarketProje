using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganikMarketProje.Models;
using OrganikMarketProje.Services;

namespace OrganikMarketProje.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductOperations _productOps;

        public ProductController(ProductOperations productOps)
        {
            _productOps = productOps;
        }

        public IActionResult Index()
        {
            var products = _productOps.GetAll();
            return View(products);
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
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var product = _productOps.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Product updatedProduct, IFormFile image)
        {
            var product = _productOps.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Category = updatedProduct.Category;
            product.DeliveryInfo = updatedProduct.DeliveryInfo;
            product.StockQuantity = updatedProduct.StockQuantity; // DÜZELTİLDİ

            if (image != null)
            {
                using var ms = new MemoryStream();
                await image.CopyToAsync(ms);
                product.ImageData = ms.ToArray();
                product.ImageType = image.ContentType;
            }

            _productOps.Update(product);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            _productOps.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
