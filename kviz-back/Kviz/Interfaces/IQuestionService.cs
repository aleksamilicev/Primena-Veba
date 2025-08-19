using Kviz.DTOs;

namespace Kviz.Interfaces
{
    public interface IQuestionService
    {
        Task<List<QuestionDto>> GetQuestionsByQuizIdAsync(int quizId);
        Task<QuizWithQuestionsDto> GetQuizWithQuestionsAsync(int quizId);
    }
}
