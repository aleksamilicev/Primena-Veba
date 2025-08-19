using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "Korisničko ime je obavezno")]
        //[StringLength(12, MinimumLength = 3, ErrorMessage = "Korisničko ime mora biti između 3 i 12 karaktera")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan")]
        [EmailAddress(ErrorMessage = "Neispravna email adresa")]
        //[StringLength(30, ErrorMessage = "Email ne sme biti duži od 30 karaktera")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna")]
        //[StringLength(12, MinimumLength = 6, ErrorMessage = "Lozinka mora biti između 6 i 12 karaktera")]
        public string Password { get; set; } = string.Empty;
    }

}
