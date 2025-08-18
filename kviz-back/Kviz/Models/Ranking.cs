using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    public class Ranking
    {
        public int Ranking_Id { get; set; }

        public int Quiz_Id { get; set; }

        public int User_Id { get; set; }

        public float Score_Percentage { get; set; }

        public int Time_Taken { get; set; }

        public int Rank_Position { get; set; }

        // Navigaciona svojstva
        public Quiz Quiz { get; set; }
        public User User { get; set; }
    }
}
