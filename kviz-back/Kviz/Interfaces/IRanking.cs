using Microsoft.AspNetCore.Mvc;

namespace Kviz.Interfaces
{
    public interface IRanking
    {
        Task<IActionResult> GetAllRankings([FromQuery] string? period = null);
        Task<IActionResult> GetRanking(int quizId, [FromQuery] string? period = null);


    }
}
