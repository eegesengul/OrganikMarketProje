using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrganikMarketProje.Data;
using OrganikMarketProje.Models;
using OrganikMarketProje.Services;
using System;
using System.Linq;

namespace OrganikMarketProje.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CartService _cartService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<OrderController> _logger;

        public OrderController(AppDbContext context, CartService cartService, UserManager<AppUser> userManager, ILogger<OrderController> logger)
        {
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var orders = _context.Orders
                .Where(o => o.UserName == User.Identity.Name)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id && o.UserName == User.Identity.Name);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var cartItems = _cartService.GetCart();
            if (cartItems.Count == 0)
            {
                TempData["OrderError"] = "Sepetiniz boş!";
                return RedirectToAction("Index", "Cart");
            }

            return View(new OrderViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(OrderViewModel model)
        {
            var cartItems = _cartService.GetCart();
            if (cartItems.Count == 0)
            {
                TempData["OrderError"] = "Sepetiniz boş!";
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

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
                    UserId = userId,
                    UserName = User.Identity.Name,
                    OrderDate = DateTime.Now,
                    DeliveryAddress = model.DeliveryAddress,
                    PhoneNumber = model.PhoneNumber,
                    Note = model.Note,
                    Status = "Hazırlanıyor",
                    OrderItems = cartItems.Select(c => new OrderItem
                    {
                        ProductId = c.ProductId,
                        Quantity = c.Quantity,
                        UnitPrice = c.Product.Price
                    }).ToList()
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                _logger.LogInformation($"Sipariş #{order.Id} başarıyla oluşturuldu. Kullanıcı: {User.Identity.Name}, Tarih: {order.OrderDate}, Ürün Sayısı: {order.OrderItems.Count}, Toplam Tutar: {order.OrderItems.Sum(i => i.UnitPrice * i.Quantity)} ₺");

                _cartService.ClearCart();
                TempData["OrderMessage"] = $"Siparişiniz başarıyla oluşturuldu! Sipariş Numaranız: {order.Id}, Durum: {order.Status}. Teşekkür ederiz!";

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}
