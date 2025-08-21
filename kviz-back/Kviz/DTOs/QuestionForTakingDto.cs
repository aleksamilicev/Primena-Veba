namespace Kviz.DTOs
{
    // DTO za pitanje tokom rešavanja kviza (bez tačnog odgovora)
    public class QuestionForTakingDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public string? DifficultyLevel { get; set; }
        public List<string> Options { get; set; } = new List<string>(); // Za multiple-choice i true/false
    }
}
