namespace Kviz.DTOs
{
    public class ResumeAttemptDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int RemainingQuestions { get; set; }
        public List<QuestionForTakingDto> NextQuestions { get; set; } = new List<QuestionForTakingDto>();
        public int TimeElapsed { get; set; }
        public double Progress => TotalQuestions > 0 ? (double)AnsweredQuestions / TotalQuestions * 100 : 0;
    }
}
