using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    [Table("QUESTIONS", Schema = "SKALARR")]
    public class Question
    {
        [Key]
        [Column("QUESTION_ID")]
        public int Question_Id { get; set; }

        [Column("QUIZ_ID")]
        public int Quiz_Id { get; set; }

        [Required]
        [Column("QUESTION_TEXT")]
        public string Question_Text { get; set; } = string.Empty;

        [Column("QUESTION_TYPE")]
        [MaxLength(255)]
        public string? Question_Type { get; set; }

        [Column("CORRECT_ANSWER")]
        public string? Correct_Answer { get; set; }

        [Column("DIFFICULTY_LEVEL")]
        [MaxLength(255)]
        public string? Difficulty_Level { get; set; }

        // Navigation properties
        [ForeignKey("Quiz_Id")]
        public virtual Quiz Quiz { get; set; } = null!;
    }
}