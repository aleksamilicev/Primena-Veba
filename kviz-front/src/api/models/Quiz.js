// src/api/models/Quiz.js
class Quiz {
  constructor(quizId, title, description, numberOfQuestions, category, difficultyLevel, timeLimit) {
    this.quizId = quizId;
    this.title = title;
    this.description = description;
    this.numberOfQuestions = numberOfQuestions;
    this.category = category;
    this.difficultyLevel = difficultyLevel;
    this.timeLimit = timeLimit;
  }
}

export default Quiz;
