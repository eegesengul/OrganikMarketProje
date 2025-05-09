using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public AppUser? User { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        [Required]
        public List<OrderItem> OrderItems { get; set; } = new();

        [Required]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Note { get; set; }

        [Required]
        public string Status { get; set; } = "Hazırlanıyor";  // EKLENEN ALAN
    }
}
