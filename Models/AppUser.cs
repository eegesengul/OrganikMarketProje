using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class AppUser : IdentityUser
    {
        [Required(ErrorMessage = "İsim alanı zorunludur.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyisim alanı zorunludur.")]
        public string Surname { get; set; } = string.Empty;
    }
}
