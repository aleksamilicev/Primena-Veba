namespace Kviz.DTOs
{
    public class QuizAverageDto
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public double AverageScore { get; set; }
        public int ParticipantCount { get; set; }
        public string AverageScoreDisplay => $"{AverageScore:F1}%";
    }
}
