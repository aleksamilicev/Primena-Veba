export class Question {
  constructor({ quizId, questionText, questionType, difficultyLevel, correctAnswer }) {
    this.quizId = quizId;
    this.questionText = questionText;
    this.questionType = questionType;
    this.difficultyLevel = difficultyLevel;
    this.correctAnswer = correctAnswer;
  }
}
