using System.ComponentModel.DataAnnotations;

namespace Kviz.DTOs
{
    public class DeleteQuestionsDto
    {
        [Required(ErrorMessage = "Lista ID-jeva pitanja je obavezna")]
        public List<int> QuestionIds { get; set; }
    }
}
