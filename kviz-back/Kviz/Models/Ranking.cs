using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    [Table("RANKING", Schema = "SKALARR")]
    public class Ranking
    {
        [Key]
        [Column("RANKING_ID")]
        public int Ranking_Id { get; set; }

        [Column("QUIZ_ID")]
        public int Quiz_Id { get; set; }

        [Column("USER_ID")]
        public int User_Id { get; set; }

        [Column("SCORE_PERCENTAGE")]
        public float Score_Percentage { get; set; }

        [Column("TIME_TAKEN")]
        public int Time_Taken { get; set; }

        [Column("RANK_POSITION")]
        public int Rank_Position { get; set; }

        // Navigation properties
        [ForeignKey("User_Id")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("Quiz_Id")]
        public virtual Quiz Quiz { get; set; } = null!;
    }
}