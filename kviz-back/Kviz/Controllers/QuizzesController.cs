using Kviz.DTOs;
using Kviz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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




    }
}
