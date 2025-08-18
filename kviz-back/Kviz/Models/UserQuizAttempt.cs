using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    public class UserQuizAttempt
    {
        public int Attempt_Id { get; set; }

        public int User_Id { get; set; }

        public int Quiz_Id { get; set; }

        public int Attempt_Number { get; set; }

        public DateTime Attempt_Date { get; set; }

        // Navigaciona svojstva
        public User User { get; set; }
        public Quiz Quiz { get; set; }
    }
}
