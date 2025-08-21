using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Kviz.Models;
using Kviz.DTOs;

namespace Kviz.Controllers
{
    [ApiController]
    [Route("api/active-attempts")]
    [Authorize]
    public class ActiveAttemptsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ActiveAttemptsController> _logger;

        public ActiveAttemptsController(AppDbContext context, ILogger<ActiveAttemptsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/active-attempts - Svi aktivni (nedovršeni) pokušaji korisnika
        [HttpGet]
        public async Task<IActionResult> GetMyActiveAttempts()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                // Dobij sve pokušaje koji nemaju odgovarajući rezultat (nedovršeni)
                var activeAttempts = await _context.UserQuizAttempts
                    .Include(a => a.Quiz)
                    .Where(a => a.User_Id == userId &&
                              !_context.QuizResults.Any(r => r.Attempt_Id == a.Attempt_Id))
                    .OrderByDescending(a => a.Attempt_Date)
                    .Select(a => new ActiveAttemptDto
                    {
                        AttemptId = a.Attempt_Id,
                        QuizId = a.Quiz_Id,
                        QuizTitle = a.Quiz.Title,
                        AttemptNumber = a.Attempt_Number,
                        StartedAt = (DateTime)a.Attempt_Date,
                        TotalQuestions = _context.Questions.Count(q => q.Quiz_Id == a.Quiz_Id),
                        AnsweredQuestions = _context.Answers
                            .Where(ans => ans.User_Id == userId)
                            .Join(_context.Questions,
                                  answer => answer.Question_Id,
                                  question => question.Question_Id,
                                  (answer, question) => question)
                            .Count(q => q.Quiz_Id == a.Quiz_Id),
                        // Replace this line in GetMyActiveAttempts() and ResumeAttempt():
                        // TimeElapsed = (int)(DateTime.UtcNow - a.Attempt_Date).TotalSeconds

                        // with the following null-safe calculation:
                        TimeElapsed = a.Attempt_Date.HasValue ? (int)(DateTime.UtcNow - a.Attempt_Date.Value).TotalSeconds : 0
                        //TimeElapsed = (int)(DateTime.UtcNow - a.Attempt_Date).TotalSeconds
                    })
                    .ToListAsync();

                return Ok(activeAttempts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju aktivnih pokušaja za korisnika {UserId}", userId);
                return StatusCode(500, $"Greška pri dohvatanju aktivnih pokušaja: {ex.Message}");
            }
        }

        // GET: api/active-attempts/{attemptId}/resume - Nastavi pokušaj
        [HttpGet("{attemptId}/resume")]
        public async Task<IActionResult> ResumeAttempt(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                // Proveri da li pokušaj pripada korisniku i da nije završen
                var attempt = await _context.UserQuizAttempts
                    .Include(a => a.Quiz)
                    .FirstOrDefaultAsync(a => a.Attempt_Id == attemptId && a.User_Id == userId);

                if (attempt == null)
                    return NotFound("Pokušaj ne postoji ili ne pripada korisniku");

                // Proveri da li je pokušaj završen
                var isCompleted = await _context.QuizResults
                    .AnyAsync(r => r.Attempt_Id == attemptId);

                if (isCompleted)
                    return BadRequest("Ovaj pokušaj je već završen");

                // Dobij pitanja
                var allQuestions = await _context.Questions
                    .Where(q => q.Quiz_Id == attempt.Quiz_Id)
                    .ToListAsync();

                // Dobij odgovorena pitanja
                var answeredQuestionIds = await _context.Answers
                    .Where(a => a.User_Id == userId)
                    .Join(_context.Questions,
                          answer => answer.Question_Id,
                          question => question.Question_Id,
                          (answer, question) => question.Question_Id)
                    .Where(qId => allQuestions.Select(q => q.Question_Id).Contains(qId))
                    .ToListAsync();

                // Kreiraj resume response
                var resumeData = new ResumeAttemptDto
                {
                    AttemptId = attempt.Attempt_Id,
                    QuizId = attempt.Quiz_Id,
                    QuizTitle = attempt.Quiz.Title,
                    AttemptNumber = attempt.Attempt_Number,
                    StartedAt = (DateTime)attempt.Attempt_Date,
                    TotalQuestions = allQuestions.Count,
                    AnsweredQuestions = answeredQuestionIds.Count,
                    RemainingQuestions = allQuestions.Count - answeredQuestionIds.Count,
                    NextQuestions = allQuestions
                        .Where(q => !answeredQuestionIds.Contains(q.Question_Id))
                        .Take(5) // Prikaži sledećih 5 pitanja
                        .Select(q => new QuestionForTakingDto
                        {
                            QuestionId = q.Question_Id,
                            QuestionText = q.Question_Text,
                            QuestionType = q.Question_Type,
                            DifficultyLevel = q.Difficulty_Level,
                            Options = GetOptionsForQuestion(q.Question_Type, q.Correct_Answer)
                        })
                        .ToList(),
                    // Replace this line in ResumeAttempt():
                    // TimeElapsed = (int)(DateTime.UtcNow - attempt.Attempt_Date).TotalSeconds

                    TimeElapsed = attempt.Attempt_Date.HasValue ? (int)(DateTime.UtcNow - attempt.Attempt_Date.Value).TotalSeconds : 0
                    //TimeElapsed = (int)(DateTime.UtcNow - attempt.Attempt_Date).TotalSeconds
                };

                return Ok(resumeData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri nastavku pokušaja {AttemptId} za korisnika {UserId}", attemptId, userId);
                return StatusCode(500, $"Greška pri nastavku pokušaja: {ex.Message}");
            }
        }

        // DELETE: api/active-attempts/{attemptId}/abandon - Odustani od pokušaja
        [HttpDelete("{attemptId}/abandon")]
        public async Task<IActionResult> AbandonAttempt(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                // Proveri da li pokušaj pripada korisniku
                var attempt = await _context.UserQuizAttempts
                    .FirstOrDefaultAsync(a => a.Attempt_Id == attemptId && a.User_Id == userId);

                if (attempt == null)
                    return NotFound("Pokušaj ne postoji ili ne pripada korisniku");

                // Proveri da li je pokušaj završen
                var isCompleted = await _context.QuizResults
                    .AnyAsync(r => r.Attempt_Id == attemptId);

                if (isCompleted)
                    return BadRequest("Ne možete odustati od završenog pokušaja");

                // Obriši sve odgovore za ovaj pokušaj
                var answersToDelete = await _context.Answers
                    .Where(a => a.User_Id == userId)
                    .Join(_context.Questions,
                          answer => answer.Question_Id,
                          question => question.Question_Id,
                          (answer, question) => new { answer, question })
                    .Where(joined => joined.question.Quiz_Id == attempt.Quiz_Id)
                    .Select(joined => joined.answer)
                    .ToListAsync();

                _context.Answers.RemoveRange(answersToDelete);

                // Obriši pokušaj
                _context.UserQuizAttempts.Remove(attempt);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Korisnik {userId} odustao od pokušaja {attemptId}");

                return Ok(new { message = "Pokušaj je uspešno obrisan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri odustajanju od pokušaja {AttemptId} za korisnika {UserId}", attemptId, userId);
                return StatusCode(500, $"Greška pri odustajanju od pokušaja: {ex.Message}");
            }
        }

        // Helper metode
        private List<string> GetOptionsForQuestion(string questionType, string correctAnswer)
        {
            return questionType?.ToLower() switch
            {
                "multiple-choice" => ParseMultipleChoiceOptions(correctAnswer),
                "true-false" => new List<string> { "Tačno", "Netačno" },
                _ => new List<string>()
            };
        }

        private List<string> ParseMultipleChoiceOptions(string correctAnswer)
        {
            try
            {
                if (string.IsNullOrEmpty(correctAnswer))
                    return new List<string>();

                var parts = correctAnswer.Split('|');
                return parts.Select(p => p.Trim()).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    // DTO-ovi za ActiveAttemptsController
    public class ActiveAttemptDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int TimeElapsed { get; set; } // u sekundama
        public double Progress => TotalQuestions > 0 ? (double)AnsweredQuestions / TotalQuestions * 100 : 0;
    }

    public class ResumeAttemptDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int AnsweredQuestions { get; set; }
        public int RemainingQuestions { get; set; }
        public List<QuestionForTakingDto> NextQuestions { get; set; } = new List<QuestionForTakingDto>();
        public int TimeElapsed { get; set; }
        public double Progress => TotalQuestions > 0 ? (double)AnsweredQuestions / TotalQuestions * 100 : 0;
    }
}