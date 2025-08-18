using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    public class QuizResult
    {
        public int Result_Id { get; set; }

        public int User_Id { get; set; }

        public int Quiz_Id { get; set; }

        public int Attempt_Id { get; set; }

        public int Total_Questions { get; set; }

        public int Correct_Answers { get; set; }

        public float Score_Percentage { get; set; }

        public int Time_Taken { get; set; }

        public DateTime Completed_At { get; set; }

        // Navigaciona svojstva
        public User User { get; set; }
        public Quiz Quiz { get; set; }
        public UserQuizAttempt UserQuizAttempt { get; set; }
    }
}
