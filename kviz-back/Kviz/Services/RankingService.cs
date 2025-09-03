using Kviz.DTOs;
using Kviz.Interfaces;
using Kviz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kviz.Services
{
    public class RankingService : IRanking
    {
        public Task<IActionResult> GetAllRankings([FromQuery] string? period = null)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetRanking(int quizId, [FromQuery] string? period = null)
        {
            throw new NotImplementedException();
        }
    }
}
