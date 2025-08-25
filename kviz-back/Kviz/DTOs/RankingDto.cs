namespace Kviz.DTOs
{
    // Osnovni DTO za rang listu
    public class RankingDto
    {
        public int RankingId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public float ScorePercentage { get; set; }
        public int TimeTaken { get; set; } // u sekundama
        public int RankPosition { get; set; }

        // Dodatne computed properties
        public string FormattedTime => TimeSpan.FromSeconds(TimeTaken).ToString(@"mm\:ss");
        public string ScoreDisplay => $"{ScorePercentage:F1}%";
    }
}
