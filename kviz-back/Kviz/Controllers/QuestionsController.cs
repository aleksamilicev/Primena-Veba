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

        // Postojeći endpointi - sada koriste direktno _context, OVO JE ZA ADMINE!!!
        [HttpGet("quiz/{id}/questions")]
        [Authorize]
        public async Task<ActionResult<List<QuestionAdminDto>>> GetQuizQuestions(int id)
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu videti pitanja i odgovore za kviz");
                }

                // Proveri da li kviz postoji
                var quizExists = await _context.Quizzes.FindAsync(id);
                if (quizExists == null)
                {
                    return NotFound($"Kviz sa ID {id} ne postoji");
                }

                // Dobij sva pitanja za taj kviz
                var questions = await _context.Questions
                    .Where(q => q.Quiz_Id == id)
                    .Select(q => new QuestionAdminDto
                    {
                        QuestionId = q.Question_Id,
                        QuizId = q.Quiz_Id,
                        QuestionText = q.Question_Text,
                        QuestionType = q.Question_Type,
                        DifficultyLevel = q.Difficulty_Level,
                        CorrectAnswer = q.Correct_Answer // uključeno za admin DTO
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



        // READ jedan po jedan
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

        // READ
        // Dobijanje svih pitanja
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Question>>> GetAllQuestions()
        {
            try
            {
                if (!IsCurrentUserAdmin())
                {
                    return Forbid("Samo administratori mogu videti detaljan opis pitanja");
                }

                var questions = await _context.Questions.ToListAsync();

                if (questions == null || !questions.Any())
                {
                    return NotFound("Trenutno ne postoji nijedno pitanje");
                }

                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dohvatanju svih pitanja");
                return StatusCode(500, new { message = "Greška pri dohvatanju pitanja", error = ex.Message });
            }
        }


        // Validacija za multi-select pitanja
        private bool ValidateMultiSelectFormat(string questionText, string correctAnswer)
        {
            // Proveri da li questionText sadrži opcije u formatu A:, B:, itd.
            if (!System.Text.RegularExpressions.Regex.IsMatch(questionText, @"[A-Z]:\s*\w+"))
            {
                return false;
            }

            // Proveri da li correctAnswer sadrži slova odvojena zapetama
            if (string.IsNullOrWhiteSpace(correctAnswer))
            {
                return false;
            }

            var answers = correctAnswer.Split(',').Select(a => a.Trim()).ToArray();
            return answers.All(a => System.Text.RegularExpressions.Regex.IsMatch(a, @"^[A-Z]$"));
        }

        // Validacija za one-select pitanja
        private bool ValidateOneSelectFormat(string questionText, string correctAnswer)
        {
            // Proveri da li questionText sadrži opcije u formatu A:, B:, itd.
            if (!System.Text.RegularExpressions.Regex.IsMatch(questionText, @"[A-Z]:\s*\w+"))
            {
                return false;
            }

            // Proveri da li correctAnswer je jedno slovo
            return !string.IsNullOrWhiteSpace(correctAnswer) &&
                   System.Text.RegularExpressions.Regex.IsMatch(correctAnswer.Trim(), @"^[A-Z]$");
        }

        // Metoda za validaciju pitanja po tipu
        private (bool IsValid, string ErrorMessage) ValidateQuestionByType(CreateQuestionDto dto)
        {
            var questionType = dto.QuestionType?.ToLower();

            switch (questionType)
            {
                case "true-false":
                    if (dto.CorrectAnswer?.ToLower() != "true" && dto.CorrectAnswer?.ToLower() != "false")
                    {
                        return (false, "Za tip pitanja 'true-false', tačan odgovor mora biti 'True' ili 'False'");
                    }
                    break;

                case "fill-in-the-blank":
                    if (string.IsNullOrWhiteSpace(dto.CorrectAnswer))
                    {
                        return (false, "Za tip pitanja 'fill-in-the-blank', morate uneti tačan odgovor");
                    }
                    break;

                case "multi-select":
                    if (!ValidateMultiSelectFormat(dto.QuestionText, dto.CorrectAnswer))
                    {
                        return (false, "Za tip pitanja 'multi-select', tekst pitanja mora sadržavati opcije (A:odgovor1, B:odgovor2...) i tačan odgovor mora biti u formatu poput 'A,B'");
                    }
                    break;

                case "one-select":
                    if (!ValidateOneSelectFormat(dto.QuestionText, dto.CorrectAnswer))
                    {
                        return (false, "Za tip pitanja 'one-select', tekst pitanja mora sadržavati opcije (A:odgovor1, B:odgovor2...) i tačan odgovor mora biti jedno slovo (A, B, C, D...)");
                    }
                    break;

                default:
                    return (false, "Nepoznat tip pitanja. Dozvoljeni tipovi: true-false, fill-in-the-blank, multi-select, one-select");
            }

            return (true, string.Empty);
        }


        // CREATE - Kreiranje novog pitanja (samo admin)
        [HttpPost("quiz/{quizId}")]
        [Authorize]
        public async Task<ActionResult<Question>> CreateQuestion(int quizId, [FromBody] CreateQuestionDto createQuestionDto)
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

                // Validacija tipova pitanja
                var validationResult = ValidateQuestionByType(createQuestionDto);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.ErrorMessage);
                }

                // Proveri da li kviz postoji
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                {
                    return BadRequest($"Kviz sa ID {quizId} ne postoji");
                }

                var question = new Question
                {
                    Quiz_Id = quizId,
                    Question_Text = createQuestionDto.QuestionText,
                    Question_Type = createQuestionDto.QuestionType,
                    Difficulty_Level = createQuestionDto.DifficultyLevel,
                    Correct_Answer = createQuestionDto.CorrectAnswer
                };

                _context.Questions.Add(question);

                // Ažuriramo broj pitanja u kvizu
                quiz.Number_Of_Questions++;
                _context.Quizzes.Update(quiz);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin kreirao novo pitanje sa ID {question.Question_Id} za kviz {quizId}. Kviz sada ima {quiz.Number_Of_Questions} pitanja.");

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

                // Ažuriranje polja - samo ako su poslata
                if (!string.IsNullOrEmpty(updateQuestionDto.QuestionText))
                    question.Question_Text = updateQuestionDto.QuestionText;

                if (!string.IsNullOrEmpty(updateQuestionDto.QuestionType))
                    question.Question_Type = updateQuestionDto.QuestionType;

                if (!string.IsNullOrEmpty(updateQuestionDto.DifficultyLevel))
                    question.Difficulty_Level = updateQuestionDto.DifficultyLevel;

                if (!string.IsNullOrEmpty(updateQuestionDto.CorrectAnswer))
                    question.Correct_Answer = updateQuestionDto.CorrectAnswer;

                _context.Questions.Update(question);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin ažurirao pitanje sa ID {questionId}");

                // Vraćamo DTO umesto entiteta
                var questionDto = new QuestionAdminDto
                {
                    QuestionId = question.Question_Id,
                    QuizId = question.Quiz_Id,
                    QuestionText = question.Question_Text,
                    QuestionType = question.Question_Type,
                    DifficultyLevel = question.Difficulty_Level,
                    CorrectAnswer = question.Correct_Answer
                };

                return Ok(questionDto);
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

                // Pronađi kviz da ažuriramo broj pitanja
                var quiz = await _context.Quizzes.FindAsync(question.Quiz_Id);
                if (quiz != null)
                {
                    quiz.Number_Of_Questions--;
                    _context.Quizzes.Update(quiz);
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
