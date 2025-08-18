using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    [Table("QUIZ_RESULTS", Schema = "SKALARR")]
    public class QuizResult
    {
        [Key]
        [Column("RESULT_ID")]
        public int Result_Id { get; set; }

        [Column("USER_ID")]
        public int User_Id { get; set; }

        [Column("QUIZ_ID")]
        public int Quiz_Id { get; set; }

        [Column("ATTEMPT_ID")]
        public int Attempt_Id { get; set; }

        [Column("TOTAL_QUESTIONS")]
        public int Total_Questions { get; set; }

        [Column("CORRECT_ANSWERS")]
        public int Correct_Answers { get; set; }

        [Column("SCORE_PERCENTAGE")]
        public float Score_Percentage { get; set; }

        [Column("TIME_TAKEN")]
        public int Time_Taken { get; set; }

        [Column("COMPLETED_AT")]
        public DateTime? Completed_At { get; set; }

        // Navigation properties
        [ForeignKey("User_Id")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("Quiz_Id")]
        public virtual Quiz Quiz { get; set; } = null!;

        [ForeignKey("Attempt_Id")]
        public virtual UserQuizAttempt UserQuizAttempt { get; set; } = null!;
    }
}