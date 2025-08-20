using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    public class DeleteQuizzesDto
    {
        [Required(ErrorMessage = "Lista ID-jeva kvizova je obavezna")]
        public List<int> QuizIds { get; set; }
    }
}
