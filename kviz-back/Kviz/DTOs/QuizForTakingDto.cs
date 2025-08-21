namespace Kviz.DTOs
{
    // DTO za pregled dostupnih kvizova sa dodatnim informacijama
    public class QuizForTakingDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? DifficultyLevel { get; set; }
        public int TotalQuestions { get; set; }
        public int UserAttempts { get; set; } // Broj pokušaja korisnika
        public double? BestScore { get; set; } // Najbolji rezultat korisnika
        public DateTime? LastAttempt { get; set; } // Poslednji pokušaj
        public bool CanTakeQuiz { get; set; } = true;
    }
}
