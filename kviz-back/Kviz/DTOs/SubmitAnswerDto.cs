using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    // DTO za slanje odgovora
    public class SubmitAnswerDto
    {
        [Required]
        public string UserAnswer { get; set; }   // Korisnikov odgovor
    }
}
