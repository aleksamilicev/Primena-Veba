export class RankingEntry {
  constructor(data) {
    this.rankPosition = data.Rank_Position;
    this.username = data.Username;
    this.quizId = data.Quiz_Id;
    this.scorePercentage = data.Score_Percentage;
    this.timeTaken = data.Time_Taken;
    this.completedAt = new Date(data.Completed_At);
  }
}
