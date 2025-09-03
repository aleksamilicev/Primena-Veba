using Kviz.DTOs;
using Kviz.Interfaces;
using Kviz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kviz.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizService> _logger;

        public QuestionService(AppDbContext context)
        {
            _context = context;
        }

        public Task<ActionResult<Question>> CreateQuestion(int quizId, [FromBody] CreateQuestionDto createQuestionDto)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> DeleteAllQuestionsForQuiz(int quizId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> DeleteQuestion(int questionId)
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult<IEnumerable<Question>>> GetAllQuestions()
        {
            throw new NotImplementedException();
        }

        public Task<ActionResult<Question>> GetQuestion(int questionId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<QuestionDto>> GetQuestionsByQuizIdAsync(int quizId)
        {
            var questions = await _context.Questions
                .Where(q => q.Quiz_Id == quizId)
                .Select(q => new QuestionDto
                {
                    QuestionId = q.Question_Id,
                    QuizId = q.Quiz_Id,
                    QuestionText = q.Question_Text,
                    QuestionType = q.Question_Type,
                    DifficultyLevel = q.Difficulty_Level
                })
                .ToListAsync();

            return questions;
        }

        public Task<ActionResult<List<QuestionAdminDto>>> GetQuizQuestions(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<QuizWithQuestionsDto> GetQuizWithQuestionsAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Quiz_Id == quizId);

            if (quiz == null)
                return null;

            var quizWithQuestions = new QuizWithQuestionsDto
            {
                QuizId = quiz.Quiz_Id,
                Title = quiz.Title,
                Description = quiz.Description,
                NumberOfQuestions = quiz.Number_Of_Questions,
                Category = quiz.Category,
                DifficultyLevel = quiz.Difficulty_Level,
                TimeLimit = quiz.Time_Limit,
                Questions = quiz.Questions.Select(q => new QuestionDto
                {
                    QuestionId = q.Question_Id,
                    QuizId = q.Quiz_Id,
                    QuestionText = q.Question_Text,
                    QuestionType = q.Question_Type,
                    DifficultyLevel = q.Difficulty_Level
                }).ToList()
            };

            return quizWithQuestions;
        }

        public Task<IActionResult> UpdateQuestion(int questionId, [FromBody] UpdateQuestionDto updateQuestionDto)
        {
            throw new NotImplementedException();
        }
    }
}
