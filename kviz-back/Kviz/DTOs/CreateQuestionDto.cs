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

        [Required(ErrorMessage = "Tip pitanja je obavezan")]
        [RegularExpression(@"^(true-false|fill-in-the-blank|multi-select|one-select)$",
        ErrorMessage = "Tip pitanja mora biti jedan od sledećih: true-false, fill-in-the-blank, multi-select, one-select")]
        public string? QuestionType { get; set; }

        [MaxLength(50, ErrorMessage = "Nivo težine ne može biti duži od 50 karaktera")]
        public string? DifficultyLevel { get; set; }

        [Required(ErrorMessage = "Tačan odgovor je obavezan")]
        [MaxLength(1000, ErrorMessage = "Tacan odgovor ne moze biti duzi od 1000 karaktera")]
        public string? CorrectAnswer { get; set; }
    }
}
