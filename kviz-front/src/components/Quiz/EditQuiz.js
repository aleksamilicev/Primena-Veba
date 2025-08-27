// src/components/Quiz/EditQuiz.js
import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getQuizById, updateQuizById } from "../../api/services/quizService";

const EditQuiz = () => {
  const { quizId } = useParams();
  const navigate = useNavigate();

  const [quizData, setQuizData] = useState({
    Title: "",
    Description: "",
    Category: "",
    Difficulty_Level: "",
    Time_Limit: 0
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
          Time_Limit: data.Time_Limit
        });
        setLoading(false);
      } catch (error) {
        console.error("Error fetching quiz:", error);
        setMessage("Failed to load quiz data.");
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
      alert("Quiz updated successfully!");
      navigate("/quizzes");
    } catch (error) {
      console.error("Error updating quiz:", error);
      alert("Failed to update quiz.");
    }
  };

  if (loading) return <p>Loading quiz...</p>;

  return (
    <div>
      <h2>Edit Quiz</h2>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          name="Title"
          placeholder="Title"
          value={quizData.Title}
          onChange={handleChange}
          required
        />
        <textarea
          name="Description"
          placeholder="Description"
          value={quizData.Description}
          onChange={handleChange}
        />
        <input
          type="text"
          name="Category"
          placeholder="Category"
          value={quizData.Category}
          onChange={handleChange}
        />
        <input
          type="text"
          name="Difficulty_Level"
          placeholder="Difficulty Level"
          value={quizData.Difficulty_Level}
          onChange={handleChange}
        />
        <input
          type="number"
          name="Time_Limit"
          placeholder="Time Limit (minutes)"
          value={quizData.Time_Limit}
          onChange={handleChange}
        />
        <button type="submit">Update Quiz</button>
      </form>
      {message && <p>{message}</p>}
    </div>
  );
};

export default EditQuiz;
