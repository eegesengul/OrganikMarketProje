using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "Adres alanı zorunludur.")]
        [StringLength(300, ErrorMessage = "Adres en fazla 300 karakter olabilir.")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası alanı zorunludur.")]
        [RegularExpression(@"^(\+90|0)?5\d{9}$", ErrorMessage = "Telefon numarası düzgün formatta olmalıdır.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir.")]
        public string Note { get; set; } = string.Empty;
    }
}
