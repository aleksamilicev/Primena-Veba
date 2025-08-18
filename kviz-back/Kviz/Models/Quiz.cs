using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kviz.Models
{
    public class Quiz
    {
        public int Quiz_Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        public int? Number_Of_Questions { get; set; }

        public string? Category { get; set; }

        public string? Difficulty_Level { get; set; }

        public int? Time_Limit { get; set; }

        // Navigaciona svojstva (neće biti deo baze, ali služe za povezivanje s drugim entitetima)
        public ICollection<Question>? Questions { get; set; }
    }
}
