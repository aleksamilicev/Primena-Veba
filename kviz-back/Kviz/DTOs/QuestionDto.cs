namespace Kviz.DTOs
{
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public string DifficultyLevel { get; set; }
        // Namerno ne uključujemo CorrectAnswer u DTO jer korisnik ne treba da vidi tačan odgovor
    }
}
