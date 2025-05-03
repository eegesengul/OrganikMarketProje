using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganikMarketProje.Data;
using OrganikMarketProje.Models;
using OrganikMarketProje.Services;
using System.Linq;

namespace OrganikMarketProje.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CartService _cartService;

        public OrderController(AppDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders
                .Where(o => o.UserName == User.Identity.Name)
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id && o.UserName == User.Identity.Name);

            if (order == null)
                return NotFound();

            return View(order);
        }

        public IActionResult Checkout()
        {
            var cartItems = _cartService.GetCart();
            if (cartItems.Count == 0)
            {
                TempData["OrderError"] = "Sepetiniz boş!";
                return RedirectToAction("Index", "Cart");
            }

            // Stok kontrolü ve azaltma
            foreach (var item in cartItems)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    TempData["OrderError"] = $"{product?.Name ?? "Ürün"} için yeterli stok yok. Sipariş iptal edildi.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            foreach (var item in cartItems)
            {
                var product = _context.Products.Find(item.ProductId);
                product.StockQuantity -= item.Quantity;
                _context.Products.Update(product);
            }

            var order = new Order
            {
                UserName = User.Identity.Name,
                OrderDate = DateTime.Now,
                Items = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    UnitPrice = c.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            _cartService.ClearCart();
            TempData["OrderMessage"] = "Siparişiniz başarıyla oluşturuldu!";
            return RedirectToAction("Index");
        }
    }
}
