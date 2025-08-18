using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    [Table("USER_QUIZ_ATTEMPTS", Schema = "SKALARR")]
    public class UserQuizAttempt
    {
        [Key]
        [Column("ATTEMPT_ID")]
        public int Attempt_Id { get; set; }

        [Column("USER_ID")]
        public int User_Id { get; set; }

        [Column("QUIZ_ID")]
        public int Quiz_Id { get; set; }

        [Column("ATTEMPT_NUMBER")]
        public int Attempt_Number { get; set; }

        [Column("ATTEMPT_DATE")]
        public DateTime? Attempt_Date { get; set; }

        // Navigation properties
        [ForeignKey("User_Id")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("Quiz_Id")]
        public virtual Quiz Quiz { get; set; } = null!;
    }
}