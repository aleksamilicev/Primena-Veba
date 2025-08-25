using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    // DTO za ažuriranje rang liste
    public class UpdateRankingDto
    {
        [Range(0, 100, ErrorMessage = "Procenat mora biti između 0 i 100")]
        public float? ScorePercentage { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vreme mora biti pozitivno")]
        public int? TimeTaken { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Pozicija mora biti pozitivna")]
        public int? RankPosition { get; set; }
    }
}
