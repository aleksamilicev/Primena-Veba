using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    public class LoginUserDto
    {
        public string? Username { get; set; }

        //[EmailAddress(ErrorMessage = "Neispravna email adresa")]
        public string? Email { get; set; }


        [Required(ErrorMessage = "Lozinka je obavezna")]
        public string Password { get; set; } = string.Empty;
    }
}
