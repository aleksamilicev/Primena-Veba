using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    public class Question
    {
        public int Question_Id { get; set; }

        public int Quiz_Id { get; set; }

        [Required]
        public string Question_Text { get; set; }

        public string? Question_Type { get; set; }

        public string? Correct_Answer { get; set; }

        public string? Difficulty_Level { get; set; }

        // Navigaciona svojstva
        public Quiz Quiz { get; set; }
        public ICollection<Answer>? Answers { get; set; }
    }
}
