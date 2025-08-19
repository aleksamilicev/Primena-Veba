using Kviz.DTOs;

namespace Kviz.Interfaces
{
    public interface IQuizService
    {
        Task<List<QuizDto>> GetAllQuizzesAsync();
        Task<QuizDto> GetQuizByIdAsync(int id);
        // Dodaj ostale metode koje imaš
    }
}
