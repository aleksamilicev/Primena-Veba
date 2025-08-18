using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    [Table("QUIZZES", Schema = "SKALARR")]
    public class Quiz
    {
        [Key]
        [Column("QUIZ_ID")]
        public int Quiz_Id { get; set; }

        [Required]
        [Column("TITLE")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Column("DESCRIPTION")]
        public string? Description { get; set; }

        [Column("NUMBER_OF_QUESTIONS")]
        public int? Number_Of_Questions { get; set; }

        [Column("CATEGORY")]
        [MaxLength(255)]
        public string? Category { get; set; }

        [Column("DIFFICULTY_LEVEL")]
        [MaxLength(255)]
        public string? Difficulty_Level { get; set; }

        [Column("TIME_LIMIT")]
        public int? Time_Limit { get; set; }

        // Navigation properties
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}