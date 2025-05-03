using OrganikMarketProje.Models;
using OrganikMarketProje.Helpers;
using Microsoft.AspNetCore.Http;
using OrganikMarketProje.Data;
using System.Collections.Generic;
using System.Linq;

namespace OrganikMarketProje.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private const string CartSessionKey = "CartItems";

        public CartService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private string GetUserCartKey()
        {
            var username = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Guest";
            return $"{CartSessionKey}_{username}";
        }

        public List<CartItem> GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var cart = session?.GetObjectFromJson<List<CartItem>>(GetUserCartKey()) ?? new List<CartItem>();
            return cart;
        }

        public void SaveCart(List<CartItem> cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.SetObjectAsJson(GetUserCartKey(), cart);
        }

        public bool AddToCart(Product product, int quantity, out string message)
        {
            message = "";

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == product.Id);

            int toplamIstenen = quantity;
            if (existingItem != null)
            {
                toplamIstenen += existingItem.Quantity;
            }

            if (product.StockQuantity < toplamIstenen)
            {
                message = $"Stok yetersiz. Mevcut stok: {product.StockQuantity} adet.";
                return false;
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem { ProductId = product.Id, Product = product, Quantity = quantity });
            }

            SaveCart(cart);
            message = $"{product.Name} sepete {quantity} adet eklendi.";
            return true;
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
            }
            SaveCart(cart);
        }

        public void ClearCart()
        {
            SaveCart(new List<CartItem>());
        }
    }
}
