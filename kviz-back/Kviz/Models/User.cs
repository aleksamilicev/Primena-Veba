using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    [Table("USERS", Schema = "SKALARR")]
    public class User
    {
        [Key]
        [Column("USER_ID")]
        public int User_Id { get; set; }

        [Required]
        [Column("USERNAME")]
        [MaxLength(255)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("IS_ADMIN")]
        public int Is_Admin { get; set; } = 0; // 0 = korisnik, 1 = admin


        [Required]
        [Column("EMAIL")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("PASSWORD_HASH")]
        [MaxLength(255)]
        public string Password_Hash { get; set; } = string.Empty;

        [Column("PROFILE_IMAGE_URL")]
        [MaxLength(255)]
        public string? Profile_Image_Url { get; set; }
    }
}
