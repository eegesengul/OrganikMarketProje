using System;
using System.Collections.Generic;

namespace OrganikMarketProje.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty; // Siparişi veren kullanıcının adı
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
