using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    public class CreateQuestionDto
    {
        [Required(ErrorMessage = "ID kviza je obavezan")]
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Tekst pitanja je obavezan")]
        [MaxLength(1000, ErrorMessage = "Tekst pitanja ne može biti duži od 1000 karaktera")]
        public string QuestionText { get; set; }

        [MaxLength(50, ErrorMessage = "Tip pitanja ne može biti duži od 50 karaktera")]
        public string? QuestionType { get; set; }

        [MaxLength(50, ErrorMessage = "Nivo težine ne može biti duži od 50 karaktera")]
        public string? DifficultyLevel { get; set; }
    }
}
