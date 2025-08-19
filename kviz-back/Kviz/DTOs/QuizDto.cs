namespace Kviz.DTOs
{
    public class QuizDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? NumberOfQuestions { get; set; }
        public string Category { get; set; }
        public string DifficultyLevel { get; set; }
        public int? TimeLimit { get; set; }
    }
}
