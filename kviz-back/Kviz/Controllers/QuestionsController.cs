using Kviz.DTOs;
using Kviz.Interfaces;
using Kviz.Models;
using Kviz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kviz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuizService quizService, IQuestionService questionService)
        {
            _quizService = quizService;
            _questionService = questionService;
        }

        [HttpGet("{id}/questions")]
        public async Task<ActionResult<List<QuestionDto>>> GetQuizQuestions(int id)
        {
            try
            {
                var questions = await _questionService.GetQuestionsByQuizIdAsync(id);

                if (questions == null || !questions.Any())
                {
                    return NotFound($"Nisu pronađena pitanja za kviz sa ID: {id}");
                }

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri dohvatanju pitanja: {ex.Message}");
            }
        }

        [HttpGet("{id}/with-questions")]
        public async Task<ActionResult<QuizWithQuestionsDto>> GetQuizWithQuestions(int id)
        {
            try
            {
                var quizWithQuestions = await _questionService.GetQuizWithQuestionsAsync(id);

                if (quizWithQuestions == null)
                {
                    return NotFound($"Kviz sa ID: {id} nije pronađen");
                }

                return Ok(quizWithQuestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška pri dohvatanju kviza sa pitanjima: {ex.Message}");
            }
        }
    }
}
