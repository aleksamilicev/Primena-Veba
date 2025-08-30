// src/components/Quiz/QuizCard.js
import React from "react";
import { Link } from 'react-router-dom';

const QuizCard = ({ quiz }) => {
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

      <Link 
        to={`/quiz/${quiz.Quiz_Id}/take`}
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
      </Link>
    </div>
  );
};

export default QuizCard; // ← mora biti default export
