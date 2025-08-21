namespace Kviz.DTOs
{
    public class QuizUserStatsDto
    {
        public int TotalAttempts { get; set; }
        public float? BestScore { get; set; }
        public double? AverageScore { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public float? LastScore { get; set; }
    }
}
