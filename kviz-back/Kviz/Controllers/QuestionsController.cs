using Kviz.DTOs;
using Kviz.Interfaces;
using Kviz.Models;
using Kviz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kviz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(AppDbContext context, ILogger<QuestionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Postojeći endpointi - sada koriste direktno _context, OVO JE ZA KORISNIKE!!! I NE TREBA DA SE VIDI CORRECT ANSWER
        [HttpGet("{id}/questions")]
        public async Task<ActionResult<List<QuestionDto>>> GetQuizQuestions(int id)
        {
            try
            {
                // Proveri da li kviz postoji
                var quizExists = await _context.Quizzes.FindAsync(id);
                if (quizExists == null)
                {
                    return NotFound($"Kviz sa ID {id} ne postoji");
                }

                // Dobij sva pitanja za taj kviz
                var questions = await _context.Questions
                    .Where(q => q.Quiz_Id == id)
                    .Select(q => new QuestionDto
                    {
                        QuestionId = q.Question_Id,
                        QuizId = q.Quiz_Id,
                        QuestionText = q.Question_Text,
                        QuestionType = q.Question_Type,
                        DifficultyLevel = q.Difficulty_Level
                    })
                    .ToListAsync();

                if (!questions.Any())
                {
                    return NotFound($"Nisu pronađena pitanja za kviz sa ID: {id}");
                }

                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri dohvatanju pitanja za kviz {id}");
                return StatusCode(500, $"Greška pri dohvatanju pitanja: {ex.Message}");
            }
        }

        /*
        // nisam bas siguran da li je ovo potrebno, ali ako jeste, onda je ovo endpoint za dobijanje kviza sa pitanjima
        [HttpGet("{id}/with-questions")]
        public async Task<ActionResult<QuizWithQuestionsDto>> GetQuizWithQuestions(int id)
        {
            try
            {
                var quiz = await _context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync(q => q.Quiz_Id == id);

                if (quiz == null)
                {
                    return NotFound($"Kviz sa ID: {id} nije pronađen");
                }

                var quizWithQuestions = new QuizWithQuestionsDto
                {
                    QuizId = quiz.Quiz_Id,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    Category = quiz.Category,
                    DifficultyLevel = quiz.Difficulty_Level,
                    Questions = quiz.Questions?.Select(q => new QuestionDto
                    {
                        QuestionId = q.Question_Id,
                        QuizId = q.Quiz_Id,
                        QuestionText = q.Question_Text,
                        QuestionType = q.Question_Type,
                        DifficultyLevel = q.Difficulty_Level,
                        CorrectAnswer = q.Correct_Answer
                    }).ToList() ?? new List<QuestionDto>()
                };

                return Ok(quizWithQuestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri dohvatanju kviza sa pitanjima {id}");
                return StatusCode(500, $"Greška pri dohvatanju kviza sa pitanjima: {ex.Message}");
            }
        }*/



        // READ
        // Dobijanje pojedinačnog pitanja po ID
        [HttpGet("question/{questionId}")]
        [Authorize]
        public async Task<ActionResult<Question>> GetQuestion(int questionId)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu videti detaljan opis pitanja");
                }

                var question = await _context.Questions.FindAsync(questionId);
                if (question == null)
                {
                    return NotFound($"Pitanje sa ID {questionId} ne postoji");
                }

                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri dohvatanju pitanja sa ID {questionId}");
                return StatusCode(500, new { message = "Greška pri dohvatanju pitanja", error = ex.Message });
            }
        }


        // ==================== ADMIN CRUD OPERACIJE ====================

        // CREATE - Kreiranje novog pitanja (samo admin)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Question>> CreateQuestion([FromBody] CreateQuestionDto createQuestionDto)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu kreirati pitanja");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Proveri da li kviz postoji
                var quizExists = await _context.Quizzes.FindAsync(createQuestionDto.QuizId);
                if (quizExists == null)
                {
                    return BadRequest($"Kviz sa ID {createQuestionDto.QuizId} ne postoji");
                }

                var question = new Question
                {
                    Quiz_Id = createQuestionDto.QuizId,
                    Question_Text = createQuestionDto.QuestionText,
                    Question_Type = createQuestionDto.QuestionType,
                    Difficulty_Level = createQuestionDto.DifficultyLevel,
                    Correct_Answer = createQuestionDto.CorrectAnswer
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin kreirao novo pitanje sa ID {question.Question_Id} za kviz {createQuestionDto.QuizId}");

                // Vraćamo DTO umesto entiteta da izbegnemo circular reference
                var questionDto = new QuestionAdminDto
                {
                    QuestionId = question.Question_Id,
                    QuizId = question.Quiz_Id,
                    QuestionText = question.Question_Text,
                    QuestionType = question.Question_Type,
                    DifficultyLevel = question.Difficulty_Level,
                    CorrectAnswer = question.Correct_Answer
                };

                return CreatedAtAction(nameof(GetQuestion), new { questionId = question.Question_Id }, questionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri kreiranju pitanja");
                return StatusCode(500, new { message = "Greška pri kreiranju pitanja", error = ex.Message });
            }
        }



        // UPDATE - Ažuriranje postojećeg pitanja (samo admin)
        [HttpPut("question/{questionId}")]
        [Authorize]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] UpdateQuestionDto updateQuestionDto)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu ažurirati pitanja");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var question = await _context.Questions.FindAsync(questionId);
                if (question == null)
                {
                    return NotFound($"Pitanje sa ID {questionId} ne postoji");
                }

                // Ako se menja kviz, proveri da li novi kviz postoji
                if (updateQuestionDto.QuizId.HasValue && updateQuestionDto.QuizId != question.Quiz_Id)
                {
                    var newQuizExists = await _context.Quizzes.FindAsync(updateQuestionDto.QuizId.Value);
                    if (newQuizExists == null)
                    {
                        return BadRequest($"Kviz sa ID {updateQuestionDto.QuizId} ne postoji");
                    }
                    question.Quiz_Id = updateQuestionDto.QuizId.Value;
                }

                // Ažuriranje polja
                question.Question_Text = updateQuestionDto.QuestionText ?? question.Question_Text;
                question.Question_Type = updateQuestionDto.QuestionType ?? question.Question_Type;
                question.Difficulty_Level = updateQuestionDto.DifficultyLevel ?? question.Difficulty_Level;

                _context.Questions.Update(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin ažurirao pitanje sa ID {questionId}");

                return Ok(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri ažuriranju pitanja sa ID {questionId}");
                return StatusCode(500, new { message = "Greška pri ažuriranju pitanja", error = ex.Message });
            }
        }


        // DELETE - Brisanje pitanja (samo admin)
        [HttpDelete("question/{questionId}")]
        [Authorize]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu brisati pitanja");
                }

                var question = await _context.Questions.FindAsync(questionId);
                if (question == null)
                {
                    return NotFound($"Pitanje sa ID {questionId} ne postoji");
                }

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin obrisao pitanje sa ID {questionId}");

                return Ok(new { message = $"Pitanje sa ID {questionId} je uspešno obrisano" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri brisanju pitanja sa ID {questionId}");
                return StatusCode(500, new { message = "Greška pri brisanju pitanja", error = ex.Message });
            }
        }




        // DELETE ALL - Brisanje svih pitanja za određeni kviz (samo admin)
        [HttpDelete("quiz/{quizId}/questions")]
        [Authorize]
        public async Task<IActionResult> DeleteAllQuestionsForQuiz(int quizId)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu brisati pitanja");
                }

                // Proveri da li kviz postoji
                var quizExists = await _context.Quizzes.FindAsync(quizId);
                if (quizExists == null)
                {
                    return NotFound($"Kviz sa ID {quizId} ne postoji");
                }

                var questions = await _context.Questions
                    .Where(q => q.Quiz_Id == quizId)
                    .ToListAsync();

                if (!questions.Any())
                {
                    return NotFound($"Nema pitanja za kviz sa ID {quizId}");
                }

                var questionCount = questions.Count;
                _context.Questions.RemoveRange(questions);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin obrisao sva pitanja ({questionCount}) za kviz sa ID {quizId}");

                return Ok(new
                {
                    message = $"Uspešno obrisano {questionCount} pitanja za kviz sa ID {quizId}",
                    deletedCount = questionCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Greška pri brisanju svih pitanja za kviz sa ID {quizId}");
                return StatusCode(500, new { message = "Greška pri brisanju pitanja", error = ex.Message });
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
