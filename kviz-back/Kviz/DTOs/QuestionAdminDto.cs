namespace Kviz.DTOs
{
    public class QuestionAdminDto
    {
        public int QuestionId { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public string DifficultyLevel { get; set; }

        public string CorrectAnswer { get; set; }

    }
}
