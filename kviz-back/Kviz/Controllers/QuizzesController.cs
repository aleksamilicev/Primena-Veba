using Kviz.DTOs;
using Kviz.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kviz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizzesController> _logger;

        public QuizzesController(AppDbContext context, ILogger<QuizzesController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // Standardni endpoint za dobijanje svih kvizova + filtriranje
        [HttpGet]
        public async Task<IActionResult> GetQuizzes(
    [FromQuery] string? category,
    [FromQuery] string? difficulty,
    [FromQuery] string? search
)
        {
            try
            {
                // Debug log za praćenje parametara
                _logger.LogInformation($"GetQuizzes pozvan sa parametrima - Category: '{category}', Difficulty: '{difficulty}', Search: '{search}'");

                var query = _context.Quizzes.AsQueryable();

                // Ako je prosleđena kategorija, filtriraj po njoj
                if (!string.IsNullOrWhiteSpace(category))
                {
                    _logger.LogInformation($"Filtriranje po kategoriji: {category}");
                    query = query.Where(q => q.Category != null &&
                                       q.Category.Trim().ToLower() == category.Trim().ToLower());
                }

                // Ako je prosleđena težina, filtriraj po njoj
                if (!string.IsNullOrWhiteSpace(difficulty))
                {
                    _logger.LogInformation($"Filtriranje po težini: {difficulty}");
                    query = query.Where(q => q.Difficulty_Level != null &&
                                       q.Difficulty_Level.Trim().ToLower() == difficulty.Trim().ToLower());
                }

                // Ako je prosleđena ključna reč, pretražuj u title i description
                if (!string.IsNullOrWhiteSpace(search))
                {
                    _logger.LogInformation($"Pretraga po ključnoj reči: {search}");
                    var searchTerm = search.Trim().ToLower();
                    query = query.Where(q =>
                        (q.Title != null && q.Title.ToLower().Contains(searchTerm)) ||
                        (q.Description != null && q.Description.ToLower().Contains(searchTerm))
                    );
                }

                var quizzes = await query.ToListAsync();

                _logger.LogInformation($"Pronađeno {quizzes.Count} kvizova nakon filtriranja");

                // Debug: Ispišimo prvih nekoliko kvizova da vidimo njihove vrednosti
                if (quizzes.Any())
                {
                    foreach (var quiz in quizzes.Take(3))
                    {
                        _logger.LogInformation($"Quiz ID: {quiz.Quiz_Id}, Category: '{quiz.Category}', Difficulty: '{quiz.Difficulty_Level}', Title: '{quiz.Title}', Description: '{quiz.Description?.Substring(0, Math.Min(30, quiz.Description.Length))}...'");
                    }
                }

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju kvizova");
                return StatusCode(500, new { message = "Greška pri dohvatanju kvizova", error = ex.Message });
            }
        }

        // 1. Endpoint za dobijanje pitanja od kviza po ID
        [HttpGet("{quizId}/questions")]
        public async Task<ActionResult<List<QuestionDto>>> GetQuestionsByQuizId(int quizId)
        {
            try
            {
                // Proveri da li kviz postoji
                var quizExists = await _context.Quizzes.FindAsync(quizId);
                if (quizExists == null)
                {
                    return NotFound($"Kviz sa ID {quizId} ne postoji");
                }

                // Dobij sva pitanja za taj kviz
                var questions = await _context.Questions
                    .Where(q => q.Quiz_Id == quizId)
                    .Select(q => new QuestionDto
                    {
                        QuestionId = q.Question_Id,
                        QuizId = q.Quiz_Id,
                        QuestionText = q.Question_Text,
                        QuestionType = q.Question_Type,
                        DifficultyLevel = q.Difficulty_Level
                        // ovde ne vracamo CorrectAnswer jer korisnik ne treba da vidi tačan odgovor
                    })
                    .ToListAsync();

                if (questions.Count == 0)
                {
                    return NotFound($"Nema pitanja za kviz sa ID {quizId}");
                }

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška: {ex.Message}");
            }
        }

        #region Check Answer Helper Method
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
            // Format: "A) Opcija 1 | B) Opcija 2 | C) Opcija 3 [CORRECT: A]"
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
            // Format: "A,C,D" (više tačnih odgovora)
            try
            {
                var correctAnswers = correctAnswer.Split(',').Select(a => a.Trim()).OrderBy(a => a);
                var userAnswers = userAnswer.Split(',').Select(a => a.Trim()).OrderBy(a => a);

                return correctAnswers.SequenceEqual(userAnswers);
            }
            catch
            {
                return false;
            }
        }

        private bool CheckTrueFalseAnswer(string correctAnswer, string userAnswer)
        {
            var normalizedCorrect = correctAnswer.ToLower().Trim();
            var normalizedUser = userAnswer.ToLower().Trim();

            // Mapiranje različitih formata na standardne vrednosti
            var trueValues = new[] { "true", "tačno", "tacno", "да", "da", "1", "yes" };
            var falseValues = new[] { "false", "netačno", "netacno", "не", "ne", "0", "no" };

            bool isCorrectTrue = trueValues.Contains(normalizedCorrect);
            bool isUserTrue = trueValues.Contains(normalizedUser);
            bool isCorrectFalse = falseValues.Contains(normalizedCorrect);
            bool isUserFalse = falseValues.Contains(normalizedUser);

            return (isCorrectTrue && isUserTrue) || (isCorrectFalse && isUserFalse);
        }

        private bool CheckFillInBlankAnswer(string correctAnswer, string userAnswer)
        {
            // Može podržavati više varijanti odgovora odvojenih sa "|"
            var correctVariants = correctAnswer.Split('|').Select(v => v.Trim().ToLower());
            var normalizedUserAnswer = userAnswer.Trim().ToLower();

            return correctVariants.Any(variant => variant == normalizedUserAnswer);
        }
        #endregion


        [HttpPost("{quizId}/questions/{questionId}/answer")]
        public async Task<IActionResult> SubmitAnswer(int quizId, int questionId, [FromBody] SubmitAnswerDto submitAnswerDto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                // 1. Proveri kviz i pitanje
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                    return NotFound($"Kviz sa ID {quizId} ne postoji");

                var question = await _context.Questions.FindAsync(questionId);
                if (question == null)
                    return NotFound($"Pitanje sa ID {questionId} ne postoji");

                if (question.Quiz_Id != quizId)
                    return BadRequest("Pitanje ne pripada prosleđenom kvizu");

                // 2. Proveri da li je korisnik već odgovorio na ovo pitanje
                var existingAnswer = await _context.Answers
                    .Where(a => a.User_Id == userId.Value && a.Question_Id == questionId && a.Quiz_Id == quizId)
                    .FirstOrDefaultAsync();

                if (existingAnswer != null)
                    return BadRequest("Već ste odgovorili na ovo pitanje");

                // 3. Proveri tačnost
                bool isCorrect = CheckAnswer(question.Question_Type, question.Correct_Answer, submitAnswerDto.UserAnswer);

                // 4. Sačuvaj odgovor
                var answer = new Answer
                {
                    User_Id = userId.Value,
                    Question_Id = questionId,
                    Quiz_Id = quizId, // NOVO
                    User_Answer = submitAnswerDto.UserAnswer,
                    Is_Correct = isCorrect ? 1 : 0
                };

                _context.Answers.Add(answer);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    AnswerId = answer.Answer_Id,
                    IsCorrect = isCorrect,
                    QuestionId = questionId,
                    QuizId = quizId,
                    Message = isCorrect ? "Tačan odgovor!" : "Netačan odgovor."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri čuvanju odgovora: {ex.Message}");
            }
        }







        // Endpoint za dobijanje kviza po ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            try
            {
                var quiz = await _context.Quizzes.FindAsync(id);
                if (quiz == null)
                {
                    return NotFound($"Kviz sa ID {id} ne postoji");
                }

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri dohvatanju kviza sa ID {id}");
                return StatusCode(500, new { message = "Greška pri dohvatanju kviza", error = ex.Message });
            }
        }



        // ==================== ADMIN CRUD OPERACIJE ====================

        // CREATE - Kreiranje novog kviza (samo admin)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Quiz>> CreateQuiz([FromBody] CreateQuizDto createQuizDto)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu kreirati kvizove");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var quiz = new Quiz
                {
                    Title = createQuizDto.Title,
                    Description = createQuizDto.Description,
                    Category = createQuizDto.Category,
                    Difficulty_Level = createQuizDto.DifficultyLevel,
                    Number_Of_Questions = createQuizDto.NumberOfQuestions,
                    Time_Limit = createQuizDto.TimeLimit
                };

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin kreirao novi kviz sa ID {quiz.Quiz_Id}");

                return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Quiz_Id }, quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju kviza");
                return StatusCode(500, new { message = "Greška pri kreiranju kviza", error = ex.Message });
            }
        }

        // UPDATE - Ažuriranje postojećeg kviza (samo admin)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] UpdateQuizDto updateQuizDto)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu ažurirati kvizove");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var quiz = await _context.Quizzes.FindAsync(id);
                if (quiz == null)
                {
                    return NotFound($"Kviz sa ID {id} ne postoji");
                }

                // Ažuriranje polja
                quiz.Title = updateQuizDto.Title ?? quiz.Title;
                quiz.Description = updateQuizDto.Description ?? quiz.Description;
                quiz.Category = updateQuizDto.Category ?? quiz.Category;
                quiz.Difficulty_Level = updateQuizDto.DifficultyLevel ?? quiz.Difficulty_Level;
                quiz.Number_Of_Questions = updateQuizDto.NumberOfQuestions ?? quiz.Number_Of_Questions;
                quiz.Time_Limit = updateQuizDto.TimeLimit ?? quiz.Time_Limit;

                _context.Quizzes.Update(quiz);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin ažurirao kviz sa ID {id}");

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri ažuriranju kviza sa ID {id}");
                return StatusCode(500, new { message = "Greška pri ažuriranju kviza", error = ex.Message });
            }
        }

        // DELETE - Brisanje kviza (samo admin)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu brisati kvizove");
                }

                var quiz = await _context.Quizzes
                    .Include(q => q.Questions) // Uključujemo pitanja za proveru
                    .FirstOrDefaultAsync(q => q.Quiz_Id == id);

                if (quiz == null)
                {
                    return NotFound($"Kviz sa ID {id} ne postoji");
                }

                // Prvo brišemo sva pitanja povezana sa kvizom
                if (quiz.Questions != null && quiz.Questions.Any())
                {
                    _context.Questions.RemoveRange(quiz.Questions);
                    _logger.LogInformation($"Obrisano {quiz.Questions.Count()} pitanja za kviz sa ID {id}");
                }

                // Zatim brišemo kviz
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin obrisao kviz sa ID {id}");

                return Ok(new { message = $"Kviz sa ID {id} je uspešno obrisan" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri brisanju kviza sa ID {id}");
                return StatusCode(500, new { message = "Greška pri brisanju kviza", error = ex.Message });
            }
        }

        // BULK DELETE - Brisanje više kvizova odjednom (samo admin)
        [HttpDelete("bulk")]
        [Authorize]
        public async Task<IActionResult> DeleteQuizzes([FromBody] DeleteQuizzesDto deleteQuizzesDto)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu brisati kvizove");
                }

                if (deleteQuizzesDto?.QuizIds == null || !deleteQuizzesDto.QuizIds.Any())
                {
                    return BadRequest("Lista ID-jeva kvizova je obavezna");
                }

                var quizzes = await _context.Quizzes
                    .Include(q => q.Questions)
                    .Where(q => deleteQuizzesDto.QuizIds.Contains(q.Quiz_Id))
                    .ToListAsync();

                if (!quizzes.Any())
                {
                    return NotFound("Nijedan od navedenih kvizova nije pronađen");
                }

                var deletedCount = 0;
                foreach (var quiz in quizzes)
                {
                    // Prvo brišemo pitanja
                    if (quiz.Questions != null && quiz.Questions.Any())
                    {
                        _context.Questions.RemoveRange(quiz.Questions);
                    }

                    _context.Quizzes.Remove(quiz);
                    deletedCount++;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin obrisao {deletedCount} kvizova");

                return Ok(new { message = $"Uspešno obrisano {deletedCount} kvizova", deletedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri bulk brisanju kvizova");
                return StatusCode(500, new { message = "Greška pri brisanju kvizova", error = ex.Message });
            }
        }










        // ==================== Resavanje kviza ====================
        // GET: api/quizzes/available - Kvizovi dostupni za rešavanje sa statistikama korisnika
        /*
        [HttpGet("available")]
        [Authorize]
        public async Task<ActionResult<List<QuizForTakingDto>>> GetAvailableQuizzes()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var quizzes = await _context.Quizzes
                    .Select(q => new QuizForTakingDto
                    {
                        QuizId = q.Quiz_Id,
                        Title = q.Title,
                        Description = q.Description,
                        Category = q.Category,
                        DifficultyLevel = q.Difficulty_Level,
                        TotalQuestions = _context.Questions.Count(quest => quest.Quiz_Id == q.Quiz_Id),
                        UserAttempts = _context.UserQuizAttempts.Count(attempt => attempt.Quiz_Id == q.Quiz_Id && attempt.User_Id == userId),
                        BestScore = _context.QuizResults
                            .Where(r => r.Quiz_Id == q.Quiz_Id && r.User_Id == userId)
                            .Max(r => (double?)r.Score_Percentage),
                        LastAttempt = _context.UserQuizAttempts
                            .Where(a => a.Quiz_Id == q.Quiz_Id && a.User_Id == userId)
                            .OrderByDescending(a => a.Attempt_Date)
                            .Select(a => (DateTime?)a.Attempt_Date)
                            .FirstOrDefault(),
                        CanTakeQuiz = true // Možete dodati logiku za ograničavanje pristupa
                    })
                    .Where(q => q.TotalQuestions > 0) // Samo kvizovi koji imaju pitanja
                    .OrderBy(q => q.Category)
                    .ThenBy(q => q.Title)
                    .ToListAsync();

                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju dostupnih kvizova za korisnika {UserId}", userId);
                return StatusCode(500, $"Greška pri dohvatanju kvizova: {ex.Message}");
            }
        }



        // GET: api/quizzes/{quizId}/preview - Pregled kviza pre početka
        [HttpGet("{quizId}/preview")]
        [Authorize]
        public async Task<ActionResult<QuizPreviewDto>> GetQuizPreview(int quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                    return NotFound($"Kviz sa ID {quizId} ne postoji");

                var totalQuestions = await _context.Questions.CountAsync(q => q.Quiz_Id == quizId);
                if (totalQuestions == 0)
                    return BadRequest("Kviz nema pitanja i ne može se pokrenuti");

                var userAttempts = await _context.UserQuizAttempts
                    .Where(a => a.Quiz_Id == quizId && a.User_Id == userId)
                    .OrderByDescending(a => a.Attempt_Date)
                    .ToListAsync();

                var userResults = await _context.QuizResults
                    .Where(r => r.Quiz_Id == quizId && r.User_Id == userId)
                    .OrderByDescending(r => r.Score_Percentage)
                    .ToListAsync();

                var questionTypes = await _context.Questions
                    .Where(q => q.Quiz_Id == quizId)
                    .GroupBy(q => q.Question_Type)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync();

                var difficultyLevels = await _context.Questions
                    .Where(q => q.Quiz_Id == quizId)
                    .GroupBy(q => q.Difficulty_Level)
                    .Select(g => new { Level = g.Key, Count = g.Count() })
                    .ToListAsync();

                var preview = new QuizPreviewDto
                {
                    QuizId = quiz.Quiz_Id,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    Category = quiz.Category,
                    DifficultyLevel = quiz.Difficulty_Level,
                    TotalQuestions = totalQuestions,
                    QuestionTypes = questionTypes.ToDictionary(qt => qt.Type ?? "Nedefinirano", qt => qt.Count),
                    DifficultyBreakdown = difficultyLevels.ToDictionary(dl => dl.Level ?? "Nedefinirano", dl => dl.Count),
                    UserStats = new QuizUserStatsDto
                    {
                        TotalAttempts = userAttempts.Count,
                        BestScore = userResults.Any() ? userResults.Max(r => r.Score_Percentage) : null,
                        AverageScore = userResults.Any() ? Math.Round(userResults.Average(r => r.Score_Percentage), 2) : null,
                        LastAttemptDate = userAttempts.FirstOrDefault()?.Attempt_Date,
                        LastScore = userResults.OrderByDescending(r => r.Completed_At).FirstOrDefault()?.Score_Percentage
                    },
                    CanStart = true,
                    EstimatedTimeMinutes = totalQuestions * 1.5 // Procena 1.5 min po pitanju
                };

                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju pregleda kviza {QuizId} za korisnika {UserId}", quizId, userId);
                return StatusCode(500, $"Greška pri dohvatanju pregleda kviza: {ex.Message}");
            }
        }

        // GET: api/quizzes/{quizId}/my-history - Istorija pokušaja korisnika za kviz
        [HttpGet("{quizId}/my-history")]
        [Authorize]
        public async Task<ActionResult<List<QuizAttemptHistoryDto>>> GetMyQuizHistory(int quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var history = await _context.UserQuizAttempts
                    .Where(a => a.Quiz_Id == quizId && a.User_Id == userId)
                    .Join(_context.QuizResults,
                          attempt => attempt.Attempt_Id,
                          result => result.Attempt_Id,
                          (attempt, result) => new QuizAttemptHistoryDto
                          {
                              AttemptId = attempt.Attempt_Id,
                              AttemptNumber = attempt.Attempt_Number,
                              AttemptDate = (DateTime)attempt.Attempt_Date,
                              Score = result.Score_Percentage,
                              CorrectAnswers = result.Correct_Answers,
                              TotalQuestions = result.Total_Questions,
                              TimeTaken = result.Time_Taken,
                              CompletedAt = (DateTime)result.Completed_At
                          })
                    .OrderByDescending(h => h.AttemptDate)
                    .ToListAsync();

                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju istorije kviza {QuizId} za korisnika {UserId}", quizId, userId);
                return StatusCode(500, $"Greška pri dohvatanju istorije: {ex.Message}");
            }
        }










        */




        // Helper metode
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private bool IsCurrentUserAdmin()
        {
            var isAdminClaim = User.FindFirst("IsAdmin")?.Value;
            return isAdminClaim == "1";
        }

    }
}
