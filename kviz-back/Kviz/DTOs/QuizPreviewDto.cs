namespace Kviz.DTOs
{
    public class QuizPreviewDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? DifficultyLevel { get; set; }
        public int TotalQuestions { get; set; }
        public Dictionary<string, int> QuestionTypes { get; set; } = new();
        public Dictionary<string, int> DifficultyBreakdown { get; set; } = new();
        public QuizUserStatsDto UserStats { get; set; }
        public bool CanStart { get; set; }
        public double EstimatedTimeMinutes { get; set; }
    }
}
