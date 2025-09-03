using Kviz.DTOs;
using Kviz.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Kviz.Controllers
{
    [ApiController]
    [Route("api/quiz-taking")]
    [Authorize]
    public class QuizTakingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizTakingController> _logger;

        public QuizTakingController(AppDbContext context, ILogger<QuizTakingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/quiz-taking/{quizId}/start - Početak kviza
        [HttpPost("{quizId}/start")]
        public async Task<IActionResult> StartQuiz(int quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                // 1. Provera da li kviz postoji
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                    return NotFound($"Kviz sa ID {quizId} ne postoji");

                // 2. Izračunaj attempt number (po korisniku i kvizu!)
                var attemptNumber = await _context.UserQuizAttempts
                    .Where(a => a.User_Id == userId.Value && a.Quiz_Id == quizId)
                    .CountAsync() + 1;

                // 3. Kreiraj novi attempt
                var attempt = new UserQuizAttempt
                {
                    User_Id = userId.Value,
                    Quiz_Id = quizId,
                    Attempt_Number = attemptNumber,
                    Attempt_Date = DateTime.UtcNow
                };

                _context.UserQuizAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                // 4. Učitaj pitanja
                var questionEntities = await _context.Questions
                    .Where(q => q.Quiz_Id == quizId)
                    .ToListAsync();

                if (!questionEntities.Any())
                    return NotFound($"Kviz {quizId} nema pitanja");

                var questions = questionEntities.Select(q =>
                {
                    // izvuci opcije
                    var options = GetOptionsForQuestion(q.Question_Type, q.Question_Text, q.Correct_Answer);

                    // očisti pitanje tako da ostane samo tekst pre "A:"
                    var firstOptionIndex = q.Question_Text.IndexOf("A:", StringComparison.OrdinalIgnoreCase);
                    string cleanQuestionText = q.Question_Text;
                    if (firstOptionIndex > 0)
                        cleanQuestionText = q.Question_Text.Substring(0, firstOptionIndex).Trim();

                    return new QuestionForTakingDto
                    {
                        QuestionId = q.Question_Id,
                        QuestionText = cleanQuestionText,
                        QuestionType = q.Question_Type,
                        DifficultyLevel = q.Difficulty_Level,
                        Options = options
                    };
                }).ToList();

                // 5. Formiraj response
                var response = new StartQuizResponseDto
                {
                    AttemptId = attempt.Attempt_Id,
                    QuizId = quizId,
                    QuizTitle = quiz.Title,
                    QuizDescription = quiz.Description,
                    AttemptNumber = attemptNumber,
                    TotalQuestions = questions.Count,
                    Questions = questions,
                    StartedAt = attempt.Attempt_Date ?? DateTime.UtcNow,
                    TimeLimit = quiz.Time_Limit
                };

                _logger.LogInformation($"Korisnik {userId} započeo kviz {quizId}, pokušaj #{attemptNumber}");

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri započinjanju kviza {QuizId} za korisnika {UserId}", quizId, userId);
                return StatusCode(500, $"Greška pri započinjanju kviza: {ex.Message}");
            }
        }





        // POST: api/quiz-taking/{quizId}/{questionId}/answer - Odgovaranje na pitanje
        [HttpPost("{quizId}/{questionId}/answer")]
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
                if (question == null || question.Quiz_Id != quizId)
                    return NotFound($"Pitanje sa ID {questionId} ne pripada kvizu {quizId}");

                // 2. Nadji aktivni attempt korisnika za ovaj kviz (najnoviji)
                var attempt = await _context.UserQuizAttempts
                    .Where(a => a.User_Id == userId.Value && a.Quiz_Id == quizId)
                    .OrderByDescending(a => a.Attempt_Date)
                    .FirstOrDefaultAsync();

                if (attempt == null)
                    return BadRequest("Niste započeli ovaj kviz");

                // 3. Proveri da li već postoji odgovor u tom attemptu
                var existingAnswer = await _context.Answers
                    .FirstOrDefaultAsync(a => a.User_Id == userId.Value &&
                                              a.Question_Id == questionId &&
                                              a.Attempt_Id == attempt.Attempt_Id);

                if (existingAnswer != null)
                    return BadRequest("Već ste odgovorili na ovo pitanje u ovom pokušaju");

                // 4. Provera tačnosti odgovora
                bool isCorrect = CheckAnswer(question.Question_Type, question.Correct_Answer, submitAnswerDto.UserAnswer);
                int correctFlag = isCorrect ? 1 : 0;

                // 5. Upis u bazu
                var answer = new Answer
                {
                    User_Id = userId.Value,
                    Quiz_Id = quizId,
                    Attempt_Id = attempt.Attempt_Id,  // veza sa attemptom
                    Question_Id = questionId,
                    User_Answer = submitAnswerDto.UserAnswer,
                    Is_Correct = correctFlag
                };

                _context.Answers.Add(answer);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Korisnik {userId} odgovorio na pitanje {questionId} (kviz {quizId}, attempt {attempt.Attempt_Id}), tačno: {isCorrect}");

                // 6. Response
                return Ok(new SubmitAnswerResponseDto
                {
                    AnswerId = answer.Answer_Id,
                    AttemptId = attempt.Attempt_Id, // može biti korisno za front
                    IsCorrect = isCorrect,
                    QuestionId = questionId,
                    Message = isCorrect ? "Tačan odgovor!" : "Netačan odgovor."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri čuvanju odgovora na pitanje {QuestionId} iz kviza {QuizId} za korisnika {UserId}",
                    questionId, quizId, userId);
                return StatusCode(500, $"Greška pri čuvanju odgovora: {ex.Message}");
            }
        }





        private async Task GenerateRankingForQuiz(int quizId)
        {
            var bestResults = await _context.QuizResults
                .Where(r => r.Quiz_Id == quizId)
                .GroupBy(r => r.User_Id)
                .Select(g => g
                    .OrderByDescending(r => r.Score_Percentage)
                    .ThenBy(r => r.Time_Taken)
                    .FirstOrDefault()
                )
                .ToListAsync();

            if (!bestResults.Any()) return;

            var ordered = bestResults
                .OrderByDescending(r => r.Score_Percentage)
                .ThenBy(r => r.Time_Taken)
                .ToList();

            var oldRanking = _context.Rankings.Where(r => r.Quiz_Id == quizId);
            _context.Rankings.RemoveRange(oldRanking);

            int position = 1;
            foreach (var result in ordered)
            {
                _context.Rankings.Add(new Ranking
                {
                    Quiz_Id = quizId,
                    User_Id = result.User_Id,
                    Score_Percentage = result.Score_Percentage,
                    Time_Taken = result.Time_Taken,
                    Rank_Position = position++,
                    Completed_At = result.Completed_At
                });
            }

            await _context.SaveChangesAsync();
        }


        // POST: api/quiz-taking/{attemptId}/finish - Završetak kviza
        [HttpPost("{attemptId}/finish")]
        public async Task<IActionResult> FinishQuiz(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                // 1. Proveri da li pokušaj pripada korisniku
                var attempt = await _context.UserQuizAttempts
                    .Include(a => a.Quiz)
                    .FirstOrDefaultAsync(a => a.Attempt_Id == attemptId && a.User_Id == userId);

                if (attempt == null)
                    return NotFound($"Pokušaj sa ID {attemptId} ne postoji ili ne pripada korisniku");

                // 2. Proveri da li rezultat već postoji
                var existingResult = await _context.QuizResults
                    .FirstOrDefaultAsync(r => r.Attempt_Id == attemptId);

                if (existingResult != null)
                    return BadRequest("Kviz je već završen za ovaj pokušaj");

                // 3. Dobij sva pitanja za kviz
                var allQuestions = await _context.Questions
                    .Where(q => q.Quiz_Id == attempt.Quiz_Id)
                    .ToListAsync();

                // 4. Dobij odgovore korisnika za ovaj attempt
                var userAnswers = await _context.Answers
                    .Where(a => a.User_Id == userId &&
                    a.Quiz_Id == attempt.Quiz_Id &&
                    a.Attempt_Id == attemptId &&    // <-- ovo je ključno
                    allQuestions.Select(q => q.Question_Id).Contains(a.Question_Id))
                    .ToListAsync();

                int totalQuestions = allQuestions.Count;
                int correctAnswers = userAnswers.Count(a => a.Is_Correct == 1);
                float scorePercentage = totalQuestions > 0
                    ? (float)correctAnswers / totalQuestions * 100
                    : 0;

                // 5. Kreiraj rezultat
                int timeTakenSeconds = 0;

                if (attempt.Attempt_Date.HasValue)
                {
                    timeTakenSeconds = (int)(DateTime.UtcNow - attempt.Attempt_Date.Value).TotalSeconds;
                }


                var result = new QuizResult
                {
                    User_Id = userId.Value,
                    Quiz_Id = attempt.Quiz_Id,
                    Attempt_Id = attemptId,
                    Total_Questions = totalQuestions,
                    Correct_Answers = correctAnswers,
                    Score_Percentage = scorePercentage,
                    Time_Taken = timeTakenSeconds,
                    Completed_At = DateTime.UtcNow
                };

                _context.QuizResults.Add(result);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear(); // reset EF tracking

                // automatski generisi ranking
                await GenerateRankingForQuiz(attempt.Quiz_Id);

                // 6. Pripremi detaljan odgovor po pitanjima
                var questionResults = allQuestions.Select(q =>
                {
                    var userAnswer = userAnswers.FirstOrDefault(a => a.Question_Id == q.Question_Id);
                    return new QuestionResultDto
                    {
                        QuestionId = q.Question_Id,
                        QuestionText = q.Question_Text,
                        CorrectAnswer = q.Correct_Answer,
                        UserAnswer = userAnswer?.User_Answer,
                        IsCorrect = userAnswer?.Is_Correct == 1,
                        WasAnswered = userAnswer != null
                    };
                }).ToList();

                // 7. Formiraj response
                var response = new FinishQuizResponseDto
                {
                    ResultId = result.Result_Id,
                    QuizTitle = attempt.Quiz.Title,
                    AttemptNumber = attempt.Attempt_Number,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswers,
                    ScorePercentage = Math.Round(scorePercentage, 2),
                    TimeTaken = timeTakenSeconds,
                    CompletedAt = result.Completed_At ?? DateTime.UtcNow,
                    QuestionResults = questionResults
                };

                _logger.LogInformation(
                    $"Korisnik {userId} završio kviz {attempt.Quiz_Id}, rezultat: {correctAnswers}/{totalQuestions} ({scorePercentage:F2}%)"
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri završetku kviza za pokušaj {AttemptId}, korisnik {UserId}", attemptId, userId);
                return StatusCode(500, $"Greška pri završetku kviza: {ex.Message}");
            }
        }


        // GET: api/quiz-taking/{attemptId}/status - Status trenutnog pokušaja
        [HttpGet("{attemptId}/status")]
        public async Task<IActionResult> GetAttemptStatus(int attemptId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var attempt = await _context.UserQuizAttempts
                    .Include(a => a.Quiz)
                    .FirstOrDefaultAsync(a => a.Attempt_Id == attemptId && a.User_Id == userId);

                if (attempt == null)
                    return NotFound($"Pokušaj sa ID {attemptId} ne postoji");

                // Proveri da li je kviz već završen
                var result = await _context.QuizResults
                    .FirstOrDefaultAsync(r => r.Attempt_Id == attemptId);

                if (result != null)
                {
                    return Ok(new AttemptStatusDto
                    {
                        AttemptId = attemptId,
                        QuizId = attempt.Quiz_Id,
                        QuizTitle = attempt.Quiz.Title,
                        IsCompleted = true,
                        CompletedAt = result.Completed_At,
                        ScorePercentage = result.Score_Percentage,
                        Message = "Kviz je završen"
                    });
                }

                // Dobij pitanja i odgovore
                var totalQuestions = await _context.Questions
                    .CountAsync(q => q.Quiz_Id == attempt.Quiz_Id);

                var answeredQuestions = await _context.Answers
                    .Where(a => a.User_Id == userId)
                    .Join(_context.Questions,
                          answer => answer.Question_Id,
                          question => question.Question_Id,
                          (answer, question) => new { answer, question })
                    .Where(joined => joined.question.Quiz_Id == attempt.Quiz_Id)
                    .CountAsync();

                return Ok(new AttemptStatusDto
                {
                    AttemptId = attemptId,
                    QuizId = attempt.Quiz_Id,
                    QuizTitle = attempt.Quiz.Title,
                    IsCompleted = false,
                    StartedAt = attempt.Attempt_Date,
                    TotalQuestions = totalQuestions,
                    AnsweredQuestions = answeredQuestions,
                    Message = $"U toku - odgovoreno {answeredQuestions}/{totalQuestions} pitanja"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dobijanju statusa pokušaja {AttemptId} za korisnika {UserId}", attemptId, userId);
                return StatusCode(500, $"Greška pri dobijanju statusa: {ex.Message}");
            }
        }

        // GET: api/quiz-taking/{quizId}/{questionId} - Dobijanje pitanja sa opcijama
        [HttpGet("{quizId}/{questionId}")]
        public async Task<IActionResult> GetQuestionWithOptions(int quizId, int questionId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var question = await _context.Questions
                    .Where(q => q.Question_Id == questionId && q.Quiz_Id == quizId)
                    .Select(q => new
                    {
                        q.Question_Id,
                        q.Question_Text,
                        q.Question_Type,
                        q.Difficulty_Level,
                        q.Correct_Answer
                    })
                    .FirstOrDefaultAsync();

                if (question == null)
                    return NotFound($"Pitanje sa ID {questionId} ne postoji u kvizu {quizId}");

                // Generiši opcije na osnovu tipa pitanja
                var options = GetOptionsForQuestion(question.Question_Type, question.Question_Text, question.Correct_Answer);

                var response = new
                {
                    QuestionId = question.Question_Id,
                    QuestionText = question.Question_Text,
                    QuestionType = question.Question_Type,
                    DifficultyLevel = question.Difficulty_Level,
                    Options = options
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dobijanju pitanja {QuestionId} iz kviza {QuizId}", questionId, quizId);
                return StatusCode(500, "Greška pri dobijanju pitanja");
            }
        }


        // Izmeni postojeći GET endpoint za kviz da uključi opcije za sva pitanja
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuizForTaking(int quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Korisnik nije autentifikovan");

            try
            {
                var quiz = await _context.Quizzes
                    .Where(q => q.Quiz_Id == quizId)
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync();

                if (quiz == null)
                    return NotFound($"Kviz sa ID {quizId} ne postoji");

                // Nadji poslednji attempt korisnika
                var latestAttempt = await _context.UserQuizAttempts
                    .Where(a => a.User_Id == userId.Value && a.Quiz_Id == quizId)
                    .OrderByDescending(a => a.Attempt_Date)
                    .FirstOrDefaultAsync();

                var questions = quiz.Questions?.Select(q => new
                {
                    QuestionId = q.Question_Id,
                    QuestionText = q.Question_Text,
                    QuestionType = q.Question_Type,
                    DifficultyLevel = q.Difficulty_Level,
                    Options = GetOptionsForQuestion(q.Question_Type, q.Question_Text, q.Correct_Answer)
                }).ToList();

                var response = new
                {
                    QuizId = quiz.Quiz_Id,
                    QuizTitle = quiz.Title,
                    AttemptNumber = latestAttempt?.Attempt_Id ?? 0,
                    Questions = questions
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Greška pri dobijanju kviza {QuizId} za korisnika {UserId}", quizId, userId);
                return StatusCode(500, "Greška pri dobijanju kviza");
            }
        }



        // Helper metode
        // Poboljšana metoda za proveru odgovora
        private bool CheckAnswer(string questionType, string correctAnswer, string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(userAnswer) || string.IsNullOrWhiteSpace(correctAnswer))
                return false;

            switch (questionType?.ToLower())
            {
                case "true-false":
                    return CheckTrueFalseAnswer(correctAnswer, userAnswer);

                case "fill-in-the-blank":
                    return CheckFillInBlankAnswer(correctAnswer, userAnswer);

                case "multi-select":
                    return CheckMultipleSelectAnswer(correctAnswer, userAnswer);

                case "one-select":
                case "multiple-choice":
                    return CheckSingleSelectAnswer(correctAnswer, userAnswer);

                default:
                    return false;
            }
        }

        // Poboljšana metoda za izvlačenje opcija iz QuestionText
        private List<string> GetOptionsForQuestion(string questionType, string questionText, string correctAnswer)
        {
            return questionType?.ToLower() switch
            {
                "multiple-choice" => ParseOptionsFromQuestionText(questionText),
                "one-select" => ParseOptionsFromQuestionText(questionText),
                "multi-select" => ParseOptionsFromQuestionText(questionText),
                "true-false" => new List<string> { "Tačno", "Netačno" },
                _ => new List<string>()
            };
        }

        private List<string> ParseOptionsFromQuestionText(string questionText)
        {
            if (string.IsNullOrEmpty(questionText))
                return new List<string>();

            try
            {
                var options = new List<string>();

                // Regex: traži A: nešto, B: nešto... sve do sledećeg slova ili kraja
                var regex = new Regex(@"([A-H]):\s*([^,]+)(?:,|$)", RegexOptions.IgnoreCase);


                var matches = regex.Matches(questionText);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        var letter = match.Groups[1].Value.ToUpper();      // A, B, C...
                        var optionText = match.Groups[2].Value.Trim().TrimEnd(',', ' ');
                        options.Add($"{letter}) {optionText}");
                    }
                }

                return options;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing options: {ex.Message}");
                return new List<string>();
            }
        }






        private bool CheckSingleSelectAnswer(string correctAnswer, string userAnswer)
        {
            // correctAnswer format: "A" ili "A,C" (uzmi prvi)
            var correctLetter = correctAnswer.Split(',')[0].Trim().ToUpper();
            var userLetter = userAnswer.Trim().ToUpper();

            return correctLetter == userLetter;
        }

        private bool CheckMultipleSelectAnswer(string correctAnswer, string userAnswer)
{
    // correctAnswer format: "A,C" 
    // userAnswer format: "A,C"
    try
    {
        var correctAnswers = correctAnswer.Split(',')
            .Select(a => a.Trim().ToUpper())
            .OrderBy(a => a)
            .ToArray();
        
        var userAnswers = userAnswer.Split(',')
            .Select(a => a.Trim().ToUpper())
            .OrderBy(a => a)
            .ToArray();

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

            // Mapiranje različitih formata
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
            var correctVariants = correctAnswer.Split('|').Select(v => v.Trim().ToLower());
            var normalizedUserAnswer = userAnswer.Trim().ToLower();

            return correctVariants.Any(variant => variant == normalizedUserAnswer);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
