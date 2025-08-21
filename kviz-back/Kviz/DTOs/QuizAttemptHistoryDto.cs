namespace Kviz.DTOs
{
    public class QuizAttemptHistoryDto
    {
        public int AttemptId { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime AttemptDate { get; set; }
        public float Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int TimeTaken { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
