using System.ComponentModel.DataAnnotations;

namespace OrganikMarketProje.Models
{
    public class EditUserViewModel
    {
        public string UserId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}
