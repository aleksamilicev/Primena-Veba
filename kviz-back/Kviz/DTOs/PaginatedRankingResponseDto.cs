namespace Kviz.DTOs
{
    // DTO za paginovane rezultate rang liste
    public class PaginatedRankingResponseDto
    {
        public List<RankingDto> Rankings { get; set; } = new List<RankingDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}
