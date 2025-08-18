using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    [Table("ANSWERS", Schema = "SKALARR")]
    public class Answer
    {
        [Key]
        [Column("ANSWER_ID")]
        public int Answer_Id { get; set; }

        [Column("USER_ID")]
        public int User_Id { get; set; }

        [Column("QUESTION_ID")]
        public int Question_Id { get; set; }

        [Column("USER_ANSWER")]
        public string? User_Answer { get; set; }

        [Column("IS_CORRECT")]
        public int Is_Correct { get; set; }

        // Navigation properties
        [ForeignKey("User_Id")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("Question_Id")]
        public virtual Question Question { get; set; } = null!;
    }
}