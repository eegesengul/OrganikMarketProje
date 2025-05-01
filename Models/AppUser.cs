using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;
    }
}
