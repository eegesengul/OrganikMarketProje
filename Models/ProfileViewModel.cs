using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşöçıİĞÜŞÖÇ\s]+$", ErrorMessage = "Ad yalnızca harflerden oluşmalıdır.")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşöçıİĞÜŞÖÇ\s]+$", ErrorMessage = "Soyad yalnızca harflerden oluşmalıdır.")]
        [StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir.")]
        public string Surname { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mevcut şifre en az 6 karakter olmalı.")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalı.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler uyuşmuyor.")]
        public string? ConfirmNewPassword { get; set; }
    }
}
