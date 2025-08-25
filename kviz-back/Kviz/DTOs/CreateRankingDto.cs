using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    // DTO za kreiranje rang liste (admin funkcionalnost)
    public class CreateRankingDto
    {
        [Required(ErrorMessage = "ID kviza je obavezan")]
        public int QuizId { get; set; }

        [Required(ErrorMessage = "ID korisnika je obavezan")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Procenat tačnosti je obavezan")]
        [Range(0, 100, ErrorMessage = "Procenat mora biti između 0 i 100")]
        public float ScorePercentage { get; set; }

        [Required(ErrorMessage = "Vreme je obavezno")]
        [Range(1, int.MaxValue, ErrorMessage = "Vreme mora biti pozitivno")]
        public int TimeTaken { get; set; }

        [Required(ErrorMessage = "Pozicija na rang listi je obavezna")]
        [Range(1, int.MaxValue, ErrorMessage = "Pozicija mora biti pozitivna")]
        public int RankPosition { get; set; }
    }

}
