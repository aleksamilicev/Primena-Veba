namespace Kviz.DTOs
{
    public class ActiveAttemptDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int TimeElapsed { get; set; } // u sekundama
        public double Progress => TotalQuestions > 0 ? (double)AnsweredQuestions / TotalQuestions * 100 : 0;
    }
}
