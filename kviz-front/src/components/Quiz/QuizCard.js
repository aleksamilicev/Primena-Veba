// src/components/Quiz/QuizCard.js
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const QuizCard = ({ quiz }) => {
  const [currentQuiz, setCurrentQuiz] = useState(null);
  const [currentAttempt, setCurrentAttempt] = useState(null);
  const [userAnswers, setUserAnswers] = useState({});
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [quizResult, setQuizResult] = useState(null);
  const [elapsedTime, setElapsedTime] = useState(0);
  const [view, setView] = useState('starting'); // 'starting', 'takingQuiz', 'results'
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();


  const resetQuizState = () => {
  console.log('Eksplicitno resetovanje stanja kviza');
  
  setCurrentQuiz(null);
  setCurrentAttempt(null);
  setUserAnswers({});
  setCurrentQuestionIndex(0);
  setQuizResult(null);
  setElapsedTime(0);
  setView('starting');
  setError(null);
  setLoading(false);
};

  const handleStartQuiz = () => {
    resetQuizState(); // Pozivaš funkciju za resetovanje stanja
    navigate(`/quiz/${quiz.Quiz_Id}/take`); // Navigacija na stranicu kviza
  };




  return (
    <div
      style={{
        border: "1px solid #ccc",
        borderRadius: "10px",
        padding: "15px",
        backgroundColor: "#f9f9f9",
      }}
    >
      <h2>{quiz.Title}</h2>
      <p>{quiz.Description}</p>
      <p>
        <strong>Category:</strong> {quiz.Category}
      </p>
      <p>
        <strong>Difficulty:</strong> {quiz.Difficulty_Level}
      </p>
      <p>
        <strong>Questions:</strong> {quiz.Number_Of_Questions}
      </p>
      <p>
        <strong>Time Limit:</strong> {quiz.Time_Limit} sec
      </p>

      <button 
        onClick={handleStartQuiz} // Kada korisnik klikne, poziva reset i navigaciju
        style={{ 
          padding: "10px 20px", 
          backgroundColor: "#28a745", 
          color: "white", 
          textDecoration: "none", 
          borderRadius: "5px",
          display: "inline-block",
          marginTop: "10px"
        }}
      >
        Rešavaj kviz
      </button>
    </div>
  );
};

export default QuizCard; // ← mora biti default export
