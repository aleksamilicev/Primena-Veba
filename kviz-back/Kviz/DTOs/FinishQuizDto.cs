using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    // DTO za završetak kviza
    public class FinishQuizDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Vreme mora biti veće od 0")]
        public int TimeTakenSeconds { get; set; }
    }
}
