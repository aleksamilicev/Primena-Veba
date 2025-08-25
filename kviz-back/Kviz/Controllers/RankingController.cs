using Kviz.DTOs;
using Kviz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

/*
 * Primer uporabe:
 * 
 * GET /api/ranking/quiz/1/top?limit=10
 * GET /api/ranking/quiz/1/my-position
 * GET /api/ranking/global?page=1&pageSize=20
 */

namespace Kviz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RankingController> _logger;

        public RankingController(AppDbContext context, ILogger<RankingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ranking
        // GET: api/ranking?period=week  - za filtriranje po periodu (week, month, all)
        [HttpGet]
        public async Task<IActionResult> GetAllRankings([FromQuery] string? period = null)
        {
            var rankingsQuery = _context.Rankings
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(period))
            {
                DateTime fromDate = period.ToLower() switch
                {
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddMonths(-1),
                    _ => DateTime.MinValue
                };

                rankingsQuery = rankingsQuery.Where(r => r.Completed_At >= fromDate);
            }

            var rankings = await rankingsQuery
                .OrderBy(r => r.Quiz_Id)
                .ThenBy(r => r.Rank_Position)
                .ToListAsync();

            if (!rankings.Any())
                return NotFound("Nema rankinga u traženom periodu");

            return Ok(rankings.Select(r => new
            {
                r.Quiz_Id,
                r.Rank_Position,
                r.User_Id,
                Username = r.User.Username,
                r.Score_Percentage,
                r.Time_Taken,
                r.Completed_At
            }));
        }


        // GET: api/ranking/{quizId}?period=week
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetRanking(int quizId, [FromQuery] string? period = null)
        {
            var rankingsQuery = _context.Rankings
                .Where(r => r.Quiz_Id == quizId)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(period))
            {
                DateTime fromDate = period.ToLower() switch
                {
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddMonths(-1),
                    _ => DateTime.MinValue
                };

                rankingsQuery = rankingsQuery.Where(r => r.Completed_At >= fromDate);
            }

            var ranking = await rankingsQuery
                .OrderBy(r => r.Rank_Position)
                .ToListAsync();

            if (!ranking.Any())
                return NotFound($"Nema rankinga za kviz {quizId} u traženom periodu");

            return Ok(ranking.Select(r => new
            {
                r.Rank_Position,
                r.User_Id,
                Username = r.User.Username,
                r.Score_Percentage,
                r.Time_Taken,
                r.Completed_At
            }));
        }
    }
}