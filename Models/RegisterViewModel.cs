using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "İsim zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşöçıİĞÜŞÖÇ\s]+$", ErrorMessage = "İsim yalnızca harflerden oluşmalıdır.")]
        [StringLength(50, ErrorMessage = "İsim en fazla 50 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyisim zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşöçıİĞÜŞÖÇ\s]+$", ErrorMessage = "Soyisim yalnızca harflerden oluşmalıdır.")]
        [StringLength(50, ErrorMessage = "Soyisim en fazla 50 karakter olabilir.")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [RegularExpression(@"^\d{10,12}$", ErrorMessage = "Telefon numarası yalnızca rakamlardan oluşmalı ve 10-12 hane arası olmalıdır.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = string.Empty;
    }
}
