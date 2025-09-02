import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import "../../styles/QuizCard.css"; // ← dodato

const QuizCard = ({ quiz }) => {
  const [currentQuiz, setCurrentQuiz] = useState(null);
  const navigate = useNavigate();

  const resetQuizState = () => {
    setCurrentQuiz(null);
  };

  const handleStartQuiz = () => {
    resetQuizState();
    navigate(`/quiz/${quiz.Quiz_Id}/take`);
  };

  return (
    <div className="quiz-card">
      <h2>{quiz.Title}</h2>
      <p>{quiz.Description}</p>
      <p><strong>Category:</strong> {quiz.Category}</p>
      <p><strong>Difficulty:</strong> {quiz.Difficulty_Level}</p>
      <p><strong>Questions:</strong> {quiz.Number_Of_Questions}</p>
      <p><strong>Time Limit:</strong> {quiz.Time_Limit} sec</p>

      <button className="start-btn" onClick={handleStartQuiz}>
        Rešavaj kviz
      </button>
    </div>
  );
};

export default QuizCard;
