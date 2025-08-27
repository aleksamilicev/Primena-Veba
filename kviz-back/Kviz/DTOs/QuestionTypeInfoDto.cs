namespace Kviz.DTOs
{
    // Dodatni DTO za vraćanje informacija o tipu pitanja frontend-u
    public class QuestionTypeInfoDto
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string CorrectAnswerFormat { get; set; }
        public string ExampleQuestionText { get; set; }
        public string ExampleCorrectAnswer { get; set; }
    }
}
