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
