export class ResultDetail {
  constructor(data) {
    this.resultId = data.resultId;
    this.quizTitle = data.quizTitle;
    this.correctAnswers = data.correctAnswers;
    this.totalQuestions = data.totalQuestions;
    this.scorePercentage = data.scorePercentage;
    this.timeTaken = data.timeTaken;
    this.completedAt = data.completedAt;
    this.answers = data.answers || [];
    this.attemptsHistory = data.attemptsHistory || [];
  }
}