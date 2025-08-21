namespace Kviz.DTOs
{
    // DTO za rezultat pojedinačnog pitanja
    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string CorrectAnswer { get; set; }
        public string? UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public bool WasAnswered { get; set; }
    }
}
