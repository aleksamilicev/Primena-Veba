using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    public class UpdateQuestionDto
    {
        public int? QuizId { get; set; }

        [MaxLength(1000, ErrorMessage = "Tekst pitanja ne može biti duži od 1000 karaktera")]
        public string? QuestionText { get; set; }

        [MaxLength(50, ErrorMessage = "Tip pitanja ne može biti duži od 50 karaktera")]
        public string? QuestionType { get; set; }


        [MaxLength(50, ErrorMessage = "Nivo težine ne može biti duži od 50 karaktera")]
        public string? DifficultyLevel { get; set; }

        [MaxLength(1000, ErrorMessage = "Tačan odgovor ne može biti duži od 1000 karaktera")]
        public string? CorrectAnswer { get; set; }
    }
}
