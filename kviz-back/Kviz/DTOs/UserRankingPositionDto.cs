namespace Kviz.DTOs
{
    // DTO za poziciju korisnika na rang listi
    public class UserRankingPositionDto : RankingDto
    {
        public int TotalParticipants { get; set; }

        // Dodatne computed properties
        public string PositionDisplay => $"{RankPosition}/{TotalParticipants}";
        public bool IsTopTen => RankPosition <= 10;
        public bool IsTopThree => RankPosition <= 3;
    }
}
