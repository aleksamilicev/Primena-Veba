using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    public class UpdateQuizDto
    {
        [MaxLength(200, ErrorMessage = "Naslov ne može biti duži od 200 karaktera")]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Opis ne može biti duži od 1000 karaktera")]
        public string? Description { get; set; }

        [MaxLength(100, ErrorMessage = "Kategorija ne može biti duža od 100 karaktera")]
        public string? Category { get; set; }

        [MaxLength(50, ErrorMessage = "Nivo težine ne može biti duži od 50 karaktera")]
        public string? DifficultyLevel { get; set; }

        [Range(1, 99, ErrorMessage = "Broj pitanja mora biti između 1 i 99")]
        public int? NumberOfQuestions { get; set; }

        [Range(1, 999, ErrorMessage = "Vreme mora biti između 1 i 999")]
        public int? TimeLimit { get; set; }
    }
}
