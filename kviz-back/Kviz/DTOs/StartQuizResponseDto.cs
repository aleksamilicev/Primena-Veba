namespace Kviz.DTOs
{
    // DTO za početak kviza
    public class StartQuizResponseDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public string? QuizDescription { get; set; }
        public int AttemptNumber { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuestionForTakingDto> Questions { get; set; } = new List<QuestionForTakingDto>();
        public DateTime StartedAt { get; set; }
    }
}
