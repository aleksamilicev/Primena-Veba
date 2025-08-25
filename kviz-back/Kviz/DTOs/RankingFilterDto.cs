namespace Kviz.DTOs
{
    // Filter parametri za rang listu
    public class RankingFilterDto
    {
        public int? QuizId { get; set; }
        public string? Period { get; set; } // "weekly", "monthly", "all"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        // Validacija
        public bool IsValidPeriod => string.IsNullOrWhiteSpace(Period) ||
                                    new[] { "weekly", "monthly", "all" }.Contains(Period.ToLower());
        public bool IsValidPageSize => PageSize > 0 && PageSize <= 100;
        public bool IsValidPage => Page > 0;
    }
}
