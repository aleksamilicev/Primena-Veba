using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Kviz.Models;
using Kviz.DTOs;

namespace Kviz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuizResultsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizResultsController> _logger;

        public QuizResultsController(AppDbContext context, ILogger<QuizResultsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/quizresults/my-results
        [HttpGet("my-results")]
        public async Task<IActionResult> GetMyResults(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? quizId = null)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var query = _context.QuizResults
                    .Include(qr => qr.Quiz)
                    .Where(qr => qr.User_Id == userId);

                if (quizId.HasValue)
                    query = query.Where(qr => qr.Quiz_Id == quizId);

                var totalCount = await query.CountAsync();

                var results = await query
                    .OrderByDescending(qr => qr.Completed_At)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(qr => new
                    {
                        resultId = qr.Result_Id,
                        quizId = qr.Quiz_Id,
                        quizTitle = qr.Quiz.Title,
                        attemptId = qr.Attempt_Id,
                        totalQuestions = qr.Total_Questions,
                        correctAnswers = qr.Correct_Answers,
                        scorePercentage = qr.Score_Percentage,
                        timeTaken = qr.Time_Taken,
                        completedAt = qr.Completed_At
                    })
                    .ToListAsync();

                return Ok(new
                {
                    results = results,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju rezultata korisnika {UserId}", userId);
                return StatusCode(500, $"Greška pri dohvatanju rezultata: {ex.Message}");
            }
        }

        // GET: api/quizresults/{resultId}
        [HttpGet("{resultId}")]
        public async Task<IActionResult> GetResultDetailss(int resultId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var result = await _context.QuizResults
                    .Include(qr => qr.Quiz)
                    .Where(qr => qr.Result_Id == resultId && qr.User_Id == userId)
                    .Select(qr => new
                    {
                        resultId = qr.Result_Id,
                        quizId = qr.Quiz_Id,
                        quizTitle = qr.Quiz.Title,
                        quizDescription = qr.Quiz.Description,
                        attemptId = qr.Attempt_Id,
                        totalQuestions = qr.Total_Questions,
                        correctAnswers = qr.Correct_Answers,
                        scorePercentage = qr.Score_Percentage,
                        timeTaken = qr.Time_Taken,
                        completedAt = qr.Completed_At
                    })
                    .FirstOrDefaultAsync();

                if (result == null)
                    return NotFound("Rezultat nije pronađen ili nemate dozvolu za pristup");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju detalja rezultata {ResultId} za korisnika {UserId}", resultId, userId);
                return StatusCode(500, $"Greška pri dohvatanju detalja rezultata: {ex.Message}");
            }
        }

        // GET: api/quizresults/{quizId}/my-results
        [HttpGet("{quizId}/my-results")]
        public async Task<IActionResult> GetMyQuizResults(int quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                // 1. Dohvati sve rezultate iz baze i materializuj u listu
                var quizResults = await _context.QuizResults
                    .Include(qr => qr.Quiz)
                    .Where(qr => qr.User_Id == userId && qr.Quiz_Id == quizId)
                    .OrderByDescending(qr => qr.Completed_At)
                    .ToListAsync();  // <--- materializacija u memoriji

                if (!quizResults.Any())
                    return NotFound("Nema rezultata za ovaj kviz");

                // 2. Projekcija sa indeksom u memoriji
                var results = quizResults.Select((qr, index) => new
                {
                    resultId = qr.Result_Id,
                    attemptId = qr.Attempt_Id,
                    attemptNumber = index + 1, // redni broj pokušaja
                    totalQuestions = qr.Total_Questions,
                    correctAnswers = qr.Correct_Answers,
                    scorePercentage = qr.Score_Percentage,
                    timeTaken = qr.Time_Taken,
                    completedAt = qr.Completed_At,
                    quizTitle = qr.Quiz.Title
                }).ToList();

                // 3. Računanje statistika
                var scores = results.Select(r => r.scorePercentage).ToList();
                var bestScore = scores.Max();
                var averageScore = scores.Average();
                var totalAttempts = results.Count;

                return Ok(new
                {
                    quizId = quizId,
                    results = results,
                    statistics = new
                    {
                        totalAttempts = totalAttempts,
                        bestScore = Math.Round(bestScore, 2),
                        averageScore = Math.Round(averageScore, 2),
                        lastAttempt = results.First()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju rezultata kviza {QuizId} za korisnika {UserId}", quizId, userId);
                return StatusCode(500, $"Greška pri dohvatanju rezultata kviza: {ex.Message}");
            }
        }


        // GET: api/quizresults/{resultId}/details
        [HttpGet("{resultId}/details")]
        public async Task<IActionResult> GetResultDetails(int resultId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                // 1. Dohvati rezultat i materializuj ga u memoriju
                var quizResult = await _context.QuizResults
                    .Include(qr => qr.Quiz)
                    .Where(qr => qr.Result_Id == resultId && qr.User_Id == userId)
                    .FirstOrDefaultAsync();

                if (quizResult == null)
                    return NotFound("Rezultat nije pronađen ili ne pripada korisniku");

                var attemptId = quizResult.Attempt_Id;

                // 2. Dohvati sve odgovore za taj attempt i materializuj u memoriju
                var answers = await _context.Answers
                    .Include(a => a.Question)
                    .Where(a => a.Attempt_Id == attemptId && a.User_Id == userId)
                    .ToListAsync(); // <--- materializacija u memoriji

                // 3. Projekcija u memoriji
                var answerDetails = answers.Select(a => new
                {
                    answerId = a.Answer_Id,
                    questionId = a.Question_Id,
                    questionText = a.Question?.Question_Text,
                    questionType = a.Question?.Question_Type,
                    correctAnswer = a.Question?.Correct_Answer,
                    userAnswer = a.User_Answer,
                    isCorrect = a.Is_Correct == 1
                }).ToList();

                // 4. Dohvati sve pokušaje korisnika za isti quiz
                var userAttempts = await _context.QuizResults
                    .Where(r => r.User_Id == userId && r.Quiz_Id == quizResult.Quiz_Id)
                    .OrderBy(r => r.Completed_At)
                    .Select(r => new {
                        attemptId = r.Attempt_Id,
                        scorePercentage = r.Score_Percentage,
                        completedAt = r.Completed_At
                    })
                    .ToListAsync();

                // 5. Sastavi response
                var response = new
                {
                    resultId = quizResult.Result_Id,
                    quizId = quizResult.Quiz_Id,
                    quizTitle = quizResult.Quiz?.Title,
                    attemptId = attemptId,
                    totalQuestions = quizResult.Total_Questions,
                    correctAnswers = quizResult.Correct_Answers,
                    scorePercentage = Math.Round(quizResult.Score_Percentage, 2),
                    timeTaken = quizResult.Time_Taken,
                    completedAt = quizResult.Completed_At,
                    answers = answerDetails,
                    attemptsHistory = userAttempts
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju detalja rezultata {ResultId} za korisnika {UserId}", resultId, userId);
                return StatusCode(500, $"Greška pri dohvatanju detalja rezultata: {ex.Message}");
            }
        }






        // GET: api/quizresults/my-stats
        [HttpGet("my-stats")]
        public async Task<IActionResult> GetMyStats()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var userResults = await _context.QuizResults
                    .Where(qr => qr.User_Id == userId)
                    .ToListAsync();

                if (!userResults.Any())
                {
                    return Ok(new
                    {
                        totalAttempts = 0,
                        quizzesAttempted = 0,
                        averageScore = 0,
                        bestScore = 0,
                        worstScore = 0,
                        totalTimeSpent = 0,
                        averageTime = 0
                    });
                }

                var scores = userResults.Select(r => r.Score_Percentage).ToList();
                var times = userResults.Select(r => r.Time_Taken).ToList();

                var stats = new
                {
                    totalAttempts = userResults.Count,
                    quizzesAttempted = userResults.Select(r => r.Quiz_Id).Distinct().Count(),
                    averageScore = Math.Round(scores.Average(), 2),
                    bestScore = Math.Round(scores.Max(), 2),
                    worstScore = Math.Round(scores.Min(), 2),
                    totalTimeSpent = times.Sum(),
                    averageTime = Math.Round(times.Average(), 2)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju statistika za korisnika {UserId}", userId);
                return StatusCode(500, $"Greška pri dohvatanju statistika: {ex.Message}");
            }
        }

        // GET: api/quizresults/recent - Poslednji rezultati korisnika
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentResults([FromQuery] int count = 5)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var recentResults = await _context.QuizResults
                    .Include(qr => qr.Quiz)
                    .Where(qr => qr.User_Id == userId)
                    .OrderByDescending(qr => qr.Completed_At)
                    .Take(count)
                    .Select(qr => new
                    {
                        resultId = qr.Result_Id,
                        quizId = qr.Quiz_Id,
                        quizTitle = qr.Quiz.Title,
                        scorePercentage = qr.Score_Percentage,
                        completedAt = qr.Completed_At,
                        timeTaken = qr.Time_Taken
                    })
                    .ToListAsync();

                return Ok(recentResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju nedavnih rezultata za korisnika {UserId}", userId);
                return StatusCode(500, $"Greška pri dohvatanju nedavnih rezultata: {ex.Message}");
            }
        }


        // ==================== ADMIN OPERACIJE ====================

        // GET: api/quizresults/admin/all-results - Admin pregled svih rezultata
        [HttpGet("admin/all-results")]
        public async Task<IActionResult> GetAllResults(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? userId = null,
            [FromQuery] int? quizId = null,
            [FromQuery] string? orderBy = "completedAt")
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid("Samo administratori mogu videti sve rezultate");
            }

            try
            {
                var query = _context.QuizResults
                    .Include(qr => qr.Quiz)
                    .Include(qr => qr.User)
                    .AsQueryable();

                // Filtriranje po korisniku
                if (userId.HasValue)
                    query = query.Where(qr => qr.User_Id == userId);

                // Filtriranje po kvizu
                if (quizId.HasValue)
                    query = query.Where(qr => qr.Quiz_Id == quizId);

                // Sortiranje
                query = orderBy?.ToLower() switch
                {
                    "score" => query.OrderByDescending(qr => qr.Score_Percentage),
                    "user" => query.OrderBy(qr => qr.User.Username),
                    "quiz" => query.OrderBy(qr => qr.Quiz.Title),
                    "time" => query.OrderByDescending(qr => qr.Time_Taken),
                    _ => query.OrderByDescending(qr => qr.Completed_At)
                };

                var totalCount = await query.CountAsync();

                var results = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(qr => new
                    {
                        resultId = qr.Result_Id,
                        userId = qr.User_Id,
                        username = qr.User.Username,
                        userEmail = qr.User.Email,
                        quizId = qr.Quiz_Id,
                        quizTitle = qr.Quiz.Title,
                        attemptId = qr.Attempt_Id,
                        totalQuestions = qr.Total_Questions,
                        correctAnswers = qr.Correct_Answers,
                        scorePercentage = qr.Score_Percentage,
                        timeTaken = qr.Time_Taken,
                        completedAt = qr.Completed_At
                    })
                    .ToListAsync();

                return Ok(new
                {
                    results = results,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    },
                    filters = new
                    {
                        userId = userId,
                        quizId = quizId,
                        orderBy = orderBy
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin greška pri dohvatanju svih rezultata");
                return StatusCode(500, $"Greška pri dohvatanju rezultata: {ex.Message}");
            }
        }

        // GET: api/quizresults/admin/user/{userId}/results - Admin pregled rezultata specifičnog korisnika
        [HttpGet("admin/user/{userId}/results")]
        public async Task<IActionResult> GetUserResultsAdmin(int userId)
        {
            if (!IsCurrentUserAdmin())
            {
                return Forbid("Samo administratori mogu videti rezultate drugih korisnika");
            }

            try
            {
                // Proveri da li korisnik postoji
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound($"Korisnik sa ID {userId} ne postoji");
                }

                var results = await _context.QuizResults
                    .Include(qr => qr.Quiz)
                    .Where(qr => qr.User_Id == userId)
                    .OrderByDescending(qr => qr.Completed_At)
                    .Select(qr => new
                    {
                        resultId = qr.Result_Id,
                        quizId = qr.Quiz_Id,
                        quizTitle = qr.Quiz.Title,
                        attemptId = qr.Attempt_Id,
                        totalQuestions = qr.Total_Questions,
                        correctAnswers = qr.Correct_Answers,
                        scorePercentage = qr.Score_Percentage,
                        timeTaken = qr.Time_Taken,
                        completedAt = qr.Completed_At
                    })
                    .ToListAsync();

                // Statistike korisnika
                var userStats = new object();
                if (results.Any())
                {
                    var scores = results.Select(r => r.scorePercentage).ToList();
                    var times = results.Select(r => r.timeTaken).ToList();

                    userStats = new
                    {
                        totalAttempts = results.Count,
                        quizzesAttempted = results.Select(r => r.quizId).Distinct().Count(),
                        averageScore = Math.Round(scores.Average(), 2),
                        bestScore = Math.Round(scores.Max(), 2),
                        worstScore = Math.Round(scores.Min(), 2),
                        totalTimeSpent = times.Sum(),
                        averageTime = Math.Round(times.Average(), 2)
                    };
                }
                else
                {
                    userStats = new
                    {
                        totalAttempts = 0,
                        quizzesAttempted = 0,
                        averageScore = 0,
                        bestScore = 0,
                        worstScore = 0,
                        totalTimeSpent = 0,
                        averageTime = 0
                    };
                }

                return Ok(new
                {
                    user = new
                    {
                        userId = user.User_Id,
                        username = user.Username,
                        email = user.Email
                    },
                    results = results,
                    statistics = userStats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin greška pri dohvatanju rezultata korisnika {UserId}", userId);
                return StatusCode(500, $"Greška pri dohvatanju rezultata korisnika: {ex.Message}");
            }
        }


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