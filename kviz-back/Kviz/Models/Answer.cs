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

        [Column("QUIZ_ID")]
        public int Quiz_Id { get; set; }

        [Column("ATTEMPT_ID")]
        public int Attempt_Id { get; set; }

        [Column("USER_ANSWER")]
        public string? User_Answer { get; set; }

        [Column("IS_CORRECT")]
        public int Is_Correct { get; set; }


        // Navigaciona polja sa explicitnim FK, ORACLE-friendly
        [ForeignKey("User_Id")]
        public virtual User? User { get; set; }

        [ForeignKey("Question_Id")]
        public virtual Question? Question { get; set; }

        [ForeignKey(nameof(Quiz_Id))]
        public virtual Quiz Quiz { get; set; }

        [ForeignKey(nameof(Attempt_Id))]
        public virtual UserQuizAttempt Attempt { get; set; }

    }
}