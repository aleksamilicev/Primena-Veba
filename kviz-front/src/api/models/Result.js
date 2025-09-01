export class Result {
  constructor(data) {
    this.resultId = data.resultId;
    this.quizId = data.quizId;
    this.quizTitle = data.quizTitle;
    this.userId = data.userId;
    this.username = data.username || null;
    this.userEmail = data.userEmail || null;
    this.correctAnswers = data.correctAnswers;
    this.totalQuestions = data.totalQuestions;
    this.scorePercentage = data.scorePercentage;
    this.timeTaken = data.timeTaken;
    this.completedAt = data.completedAt;
    this.attemptId = data.attemptId;
  }
}
