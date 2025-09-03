using Kviz.DTOs;
using Kviz.Models;

namespace Kviz.Interfaces
{
    public interface IQuizService
    {
        Task<object> SubmitAnswerAsync(int quizId, int questionId, SubmitAnswerDto submitAnswerDto, int userId);
        Task<Quiz> CreateQuizAsync(CreateQuizDto createQuizDto, int userId, bool isAdmin);
        Task<Quiz> UpdateQuizAsync(int id, UpdateQuizDto updateQuizDto, bool isAdmin);
        Task DeleteQuizAsync(int id, bool isAdmin);
        Task<int> DeleteQuizzesAsync(DeleteQuizzesDto deleteQuizzesDto, bool isAdmin);
    }
}
