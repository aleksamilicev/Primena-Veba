using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    public class UpdateProfileDto
    {
        [EmailAddress(ErrorMessage = "Neispravna email adresa")]
        [StringLength(30, ErrorMessage = "Email ne sme biti duži od 30 karaktera")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "URL ne sme biti duži od 255 karaktera")]
        public string? ProfileImageUrl { get; set; }
    }
}
