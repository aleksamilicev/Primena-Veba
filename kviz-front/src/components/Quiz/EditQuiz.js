// src/components/Quiz/EditQuiz.js
import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getQuizById, updateQuizById } from "../../api/services/quizService";
import "../../styles/EditQuiz.css";

const EditQuiz = () => {
  const { quizId } = useParams();
  const navigate = useNavigate();

  const [quizData, setQuizData] = useState({
    Title: "",
    Description: "",
    Category: "",
    Difficulty_Level: "",
    Time_Limit: 0,
  });

  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState("");

  useEffect(() => {
    const fetchQuiz = async () => {
      try {
        const data = await getQuizById(quizId);
        setQuizData({
          Title: data.Title,
          Description: data.Description,
          Category: data.Category,
          Difficulty_Level: data.Difficulty_Level,
          Time_Limit: data.Time_Limit,
        });
        setLoading(false);
      } catch (error) {
        console.error("Error fetching quiz:", error);
        setMessage("⚠️ Failed to load quiz data.");
      }
    };
    fetchQuiz();
  }, [quizId]);

  const handleChange = (e) => {
    setQuizData({ ...quizData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await updateQuizById(quizId, quizData);
      setMessage("✅ Quiz updated successfully!");
      setTimeout(() => navigate("/quizzes"), 1500);
    } catch (error) {
      console.error("Error updating quiz:", error);
      setMessage("❌ Failed to update quiz.");
    }
  };

  if (loading) return <p className="loading-text">Loading quiz...</p>;

  return (
    <div className="edit-quiz-container">
      <h2 className="edit-quiz-title">✏️ Edit Quiz</h2>
      <form onSubmit={handleSubmit} className="edit-quiz-form">
        <label>Title</label>
        <input
          type="text"
          name="Title"
          value={quizData.Title}
          onChange={handleChange}
          required
        />

        <label>Description</label>
        <textarea
          name="Description"
          value={quizData.Description}
          onChange={handleChange}
        />

        <label>Category</label>
        <input
          type="text"
          name="Category"
          value={quizData.Category}
          onChange={handleChange}
        />

        <label>Difficulty Level</label>
        <input
          type="text"
          name="Difficulty_Level"
          value={quizData.Difficulty_Level}
          onChange={handleChange}
        />

        <label>Time Limit (minutes)</label>
        <input
          type="number"
          name="Time_Limit"
          value={quizData.Time_Limit}
          onChange={handleChange}
        />

        <button type="submit" className="save-btn">💾 Save Changes</button>
      </form>
      {message && <p className="status-message">{message}</p>}
    </div>
  );
};

export default EditQuiz;
