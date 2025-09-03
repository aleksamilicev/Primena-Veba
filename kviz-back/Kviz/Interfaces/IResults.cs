using Microsoft.AspNetCore.Mvc;

namespace Kviz.Interfaces
{
    public interface IResults
    {
        Task<IActionResult> GetMyResults(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? quizId = null);
        Task<IActionResult> GetResultDetailss(int resultId);
        Task<IActionResult> GetMyQuizResults(int quizId);
        Task<IActionResult> GetResultDetails(int resultId);
        Task<IActionResult> GetMyStats();
        Task<IActionResult> GetRecentResults([FromQuery] int count = 5);
        Task<IActionResult> GetAllResults(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? userId = null,
            [FromQuery] int? quizId = null,
            [FromQuery] string? orderBy = "completedAt");
        Task<IActionResult> GetUserResultsAdmin(int userId);

    }
}
