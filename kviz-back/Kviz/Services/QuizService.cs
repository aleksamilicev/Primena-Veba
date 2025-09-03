using Kviz.DTOs;
using Kviz.Interfaces;
using Kviz.Models;
using Microsoft.EntityFrameworkCore;

namespace Kviz.Services
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizService> _logger;

        public QuizService(AppDbContext context, ILogger<QuizService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Quiz>> GetQuizzesAsync(string? category, string? difficulty, string? search, string? token)
        {
            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer "))
                throw new UnauthorizedAccessException("Niste autorizovani. Token nije prosleđen ili je neispravan.");

            var query = _context.Quizzes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(q => q.Category != null &&
                                   q.Category.Trim().ToLower() == category.Trim().ToLower());

            if (!string.IsNullOrWhiteSpace(difficulty))
                query = query.Where(q => q.Difficulty_Level != null &&
                                   q.Difficulty_Level.Trim().ToLower() == difficulty.Trim().ToLower());

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim().ToLower();
                query = query.Where(q =>
                    (q.Title != null && q.Title.ToLower().Contains(searchTerm)) ||
                    (q.Description != null && q.Description.ToLower().Contains(searchTerm))
                );
            }

            return await query.ToListAsync();
        }

        public async Task<List<QuestionDto>> GetQuestionsByQuizIdAsync(int quizId)
        {
            var quizExists = await _context.Quizzes.FindAsync(quizId);
            if (quizExists == null)
                throw new KeyNotFoundException($"Kviz sa ID {quizId} ne postoji");

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

        public async Task<object> SubmitAnswerAsync(int quizId, int questionId, SubmitAnswerDto submitAnswerDto, int userId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId)
                ?? throw new KeyNotFoundException($"Kviz sa ID {quizId} ne postoji");

            var question = await _context.Questions.FindAsync(questionId)
                ?? throw new KeyNotFoundException($"Pitanje sa ID {questionId} ne postoji");

            if (question.Quiz_Id != quizId)
                throw new ArgumentException("Pitanje ne pripada prosleđenom kvizu");

            var existingAnswer = await _context.Answers
                .Where(a => a.User_Id == userId && a.Question_Id == questionId && a.Quiz_Id == quizId)
                .FirstOrDefaultAsync();

            if (existingAnswer != null)
                throw new InvalidOperationException("Već ste odgovorili na ovo pitanje");

            bool isCorrect = CheckAnswer(question.Question_Type, question.Correct_Answer, submitAnswerDto.UserAnswer);

            var answer = new Answer
            {
                User_Id = userId,
                Question_Id = questionId,
                Quiz_Id = quizId,
                User_Answer = submitAnswerDto.UserAnswer,
                Is_Correct = isCorrect ? 1 : 0
            };

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            return new
            {
                AnswerId = answer.Answer_Id,
                IsCorrect = isCorrect,
                QuestionId = questionId,
                QuizId = quizId,
                Message = isCorrect ? "Tačan odgovor!" : "Netačan odgovor."
            };
        }

        public async Task<Quiz?> GetQuizByIdAsync(int id)
        {
            return await _context.Quizzes.FindAsync(id);
        }

        public async Task<Quiz> CreateQuizAsync(CreateQuizDto createQuizDto, int userId, bool isAdmin)
        {
            if (!isAdmin)
                throw new UnauthorizedAccessException("Samo administratori mogu kreirati kvizove");

            var quiz = new Quiz
            {
                Title = createQuizDto.Title,
                Description = createQuizDto.Description,
                Category = createQuizDto.Category,
                Difficulty_Level = createQuizDto.DifficultyLevel,
                Number_Of_Questions = 0,
                Time_Limit = createQuizDto.TimeLimit
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
            return quiz;
        }

        public async Task<Quiz> UpdateQuizAsync(int id, UpdateQuizDto updateQuizDto, bool isAdmin)
        {
            if (!isAdmin)
                throw new UnauthorizedAccessException("Samo administratori mogu ažurirati kvizove");

            var quiz = await _context.Quizzes.FindAsync(id)
                ?? throw new KeyNotFoundException($"Kviz sa ID {id} ne postoji");

            quiz.Title = updateQuizDto.Title ?? quiz.Title;
            quiz.Description = updateQuizDto.Description ?? quiz.Description;
            quiz.Category = updateQuizDto.Category ?? quiz.Category;
            quiz.Difficulty_Level = updateQuizDto.DifficultyLevel ?? quiz.Difficulty_Level;
            quiz.Number_Of_Questions = updateQuizDto.NumberOfQuestions ?? quiz.Number_Of_Questions;
            quiz.Time_Limit = updateQuizDto.TimeLimit ?? quiz.Time_Limit;

            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();

            return quiz;
        }

        public async Task DeleteQuizAsync(int id, bool isAdmin)
        {
            if (!isAdmin)
                throw new UnauthorizedAccessException("Samo administratori mogu brisati kvizove");

            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Quiz_Id == id)
                ?? throw new KeyNotFoundException($"Kviz sa ID {id} ne postoji");

            if (quiz.Questions.Any())
                _context.Questions.RemoveRange(quiz.Questions);

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteQuizzesAsync(DeleteQuizzesDto deleteQuizzesDto, bool isAdmin)
        {
            if (!isAdmin)
                throw new UnauthorizedAccessException("Samo administratori mogu brisati kvizove");

            var quizzes = await _context.Quizzes
                .Include(q => q.Questions)
                .Where(q => deleteQuizzesDto.QuizIds.Contains(q.Quiz_Id))
                .ToListAsync();

            foreach (var quiz in quizzes)
            {
                if (quiz.Questions.Any())
                    _context.Questions.RemoveRange(quiz.Questions);

                _context.Quizzes.Remove(quiz);
            }

            await _context.SaveChangesAsync();
            return quizzes.Count;
        }

        #region Helper Methods
        private bool CheckAnswer(string questionType, string correctAnswer, string userAnswer)
        {
            if (string.IsNullOrEmpty(correctAnswer) || string.IsNullOrEmpty(userAnswer))
                return false;

            return questionType?.ToLower() switch
            {
                "multiple-choice" => CheckMultipleChoiceAnswer(correctAnswer, userAnswer),
                "multiple-select" => CheckMultipleSelectAnswer(correctAnswer, userAnswer),
                "true-false" => CheckTrueFalseAnswer(correctAnswer, userAnswer),
                "fill-in-the-blank" => CheckFillInBlankAnswer(correctAnswer, userAnswer),
                _ => false
            };
        }

        private bool CheckMultipleChoiceAnswer(string correctAnswer, string userAnswer)
        {
            try
            {
                var correctPart = correctAnswer.Split("[CORRECT:").LastOrDefault()?.Replace("]", "").Trim();
                return correctPart?.Equals(userAnswer.Trim(), StringComparison.OrdinalIgnoreCase) == true;
            }
            catch
            {
                return correctAnswer.Trim().Equals(userAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private bool CheckMultipleSelectAnswer(string correctAnswer, string userAnswer)
        {
            var correctAnswers = correctAnswer.Split(',').Select(a => a.Trim()).OrderBy(a => a);
            var userAnswers = userAnswer.Split(',').Select(a => a.Trim()).OrderBy(a => a);
            return correctAnswers.SequenceEqual(userAnswers);
        }

        private bool CheckTrueFalseAnswer(string correctAnswer, string userAnswer)
        {
            var trueValues = new[] { "true", "tačno", "tacno", "da", "1", "yes" };
            var falseValues = new[] { "false", "netačno", "netacno", "ne", "0", "no" };

            return (trueValues.Contains(correctAnswer.ToLower().Trim()) &&
                    trueValues.Contains(userAnswer.ToLower().Trim()))
                || (falseValues.Contains(correctAnswer.ToLower().Trim()) &&
                    falseValues.Contains(userAnswer.ToLower().Trim()));
        }

        private bool CheckFillInBlankAnswer(string correctAnswer, string userAnswer)
        {
            var correctVariants = correctAnswer.Split('|').Select(v => v.Trim().ToLower());
            return correctVariants.Any(variant => variant == userAnswer.Trim().ToLower());
        }
        #endregion
    }
}
