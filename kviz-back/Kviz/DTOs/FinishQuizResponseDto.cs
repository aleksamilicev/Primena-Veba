namespace Kviz.DTOs
{
    // DTO za odgovor na završetak kviza
    public class FinishQuizResponseDto
    {
        public int ResultId { get; set; }
        public string QuizTitle { get; set; }
        public int AttemptNumber { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public double ScorePercentage { get; set; }
        public int TimeTaken { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<QuestionResultDto> QuestionResults { get; set; } = new List<QuestionResultDto>();
    }
}
