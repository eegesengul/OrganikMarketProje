using OrganikMarketProje.Data;
using OrganikMarketProje.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganikMarketProje.Services
{
    public class ProductOperations
    {
        private readonly AppDbContext _context;

        public ProductOperations(AppDbContext context)
        {
            _context = context;
        }

        public List<Product> GetAll() => _context.Products.ToList();

        public Product GetById(int id) => _context.Products.Find(id);

        public void Add(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                throw new InvalidOperationException("Silinmek istenen ürün bulunamadı.");

            // İlişkili tüm verileri temizle
            var cartItems = _context.CartItems.Where(ci => ci.ProductId == id);
            var orderItems = _context.OrderItems.Where(oi => oi.ProductId == id);
            var favorites = _context.FavoriteProducts.Where(fp => fp.ProductId == id);
            var comments = _context.Comments.Where(c => c.ProductId == id);
            var recipeIngredients = _context.RecipeIngredients.Where(ri => ri.ProductId == id);

            _context.CartItems.RemoveRange(cartItems);
            _context.OrderItems.RemoveRange(orderItems);
            _context.FavoriteProducts.RemoveRange(favorites);
            _context.Comments.RemoveRange(comments);
            _context.RecipeIngredients.RemoveRange(recipeIngredients);

            _context.Products.Remove(product);
            _context.SaveChanges();
        }
    }
}
