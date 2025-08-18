using System.ComponentModel.DataAnnotations;

namespace Kviz.Models
{
    public class User
    {
        public int User_Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password_Hash { get; set; }

        public string? Profile_Image_Url { get; set; }
    }
}
