namespace Kviz.DTOs
{
    // DTO za status pokušaja
    public class AttemptStatusDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double? ScorePercentage { get; set; }
        public int? TotalQuestions { get; set; }
        public int? AnsweredQuestions { get; set; }
        public string Message { get; set; }
    }
}
