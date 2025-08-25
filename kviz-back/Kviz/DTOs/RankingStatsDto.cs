namespace Kviz.DTOs
{
    // DTO za statistike rang liste
    public class RankingStatsDto
    {
        public int TotalRankings { get; set; }
        public int UniqueParticipants { get; set; }
        public int QuizzesWithRankings { get; set; }
        public List<TopPerformerDto> TopPerformers { get; set; } = new List<TopPerformerDto>();
        public List<QuizAverageDto> QuizAverages { get; set; } = new List<QuizAverageDto>();
    }
}
