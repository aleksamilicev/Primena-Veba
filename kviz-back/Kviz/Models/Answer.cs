using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    public class Answer
    {
        public int Answer_Id { get; set; }

        public int User_Id { get; set; }

        public int Question_Id { get; set; }

        public string User_Answer { get; set; }

        public bool Is_Correct { get; set; }

        // Navigaciona svojstva
        public User User { get; set; }
        public Question Question { get; set; }
    }
}
