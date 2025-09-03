using Kviz.DTOs;
using Kviz.Interfaces;
using Kviz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kviz.Services
{
    public class ResultsService : IResults
    {
        public Task<IActionResult> GetAllResults([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] int? userId = null, [FromQuery] int? quizId = null, [FromQuery] string? orderBy = "completedAt")
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetMyQuizResults(int quizId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetMyResults([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? quizId = null)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetMyStats()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetRecentResults([FromQuery] int count = 5)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetResultDetails(int resultId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetResultDetailss(int resultId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetUserResultsAdmin(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
