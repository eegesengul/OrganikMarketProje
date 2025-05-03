using Microsoft.AspNetCore.Mvc;
using OrganikMarketProje.Models;
using OrganikMarketProje.Services;
using OrganikMarketProje.Data;
using Microsoft.EntityFrameworkCore;

namespace OrganikMarketProje.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly AppDbContext _context;

        public CartController(CartService cartService, AppDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        public IActionResult Index()
        {
            var cartItems = _cartService.GetCart();
            ViewBag.Message = TempData["CartMessage"];
            ViewBag.Error = TempData["CartError"];
            return View(cartItems);
        }

        [HttpPost]
        public IActionResult Add(int productId, int quantity = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Product") });
            }

            var product = _context.Products.Find(productId);
            if (product != null)
            {
                if (_cartService.AddToCart(product, quantity, out string message))
                {
                    TempData["CartMessage"] = message;
                }
                else
                {
                    TempData["CartError"] = message;
                }
            }
            else
            {
                TempData["CartError"] = "Ürün bulunamadı.";
            }

            return RedirectToAction("Index", "Product");
        }

        public IActionResult Remove(int productId)
        {
            _cartService.RemoveFromCart(productId);
            TempData["CartMessage"] = "Ürün sepetten çıkarıldı.";
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            _cartService.ClearCart();
            TempData["CartMessage"] = "Sepet temizlendi.";
            return RedirectToAction("Index");
        }
    }
}
