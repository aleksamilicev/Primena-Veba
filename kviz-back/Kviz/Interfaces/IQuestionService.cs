using Kviz.DTOs;
using Kviz.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kviz.Interfaces
{
    public interface IQuestionService
    {
        Task<List<QuestionDto>> GetQuestionsByQuizIdAsync(int quizId);
        Task<QuizWithQuestionsDto> GetQuizWithQuestionsAsync(int quizId);

        Task<ActionResult<List<QuestionAdminDto>>> GetQuizQuestions(int id);
        Task<ActionResult<Question>> GetQuestion(int questionId);
        Task<ActionResult<IEnumerable<Question>>> GetAllQuestions();
        Task<ActionResult<Question>> CreateQuestion(int quizId, [FromBody] CreateQuestionDto createQuestionDto);
        Task<IActionResult> UpdateQuestion(int questionId, [FromBody] UpdateQuestionDto updateQuestionDto);
        Task<IActionResult> DeleteQuestion(int questionId);
        Task<IActionResult> DeleteAllQuestionsForQuiz(int quizId);

    }
}
