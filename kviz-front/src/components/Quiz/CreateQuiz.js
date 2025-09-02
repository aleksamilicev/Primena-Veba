import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { createQuiz } from "../../api/services/quizService";
import "../../styles/CreateQuiz.css";

const CreateQuiz = () => {
  const navigate = useNavigate();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [categories, setCategories] = useState([]);
  const [difficulties, setDifficulties] = useState([]);
  const [category, setCategory] = useState("");
  const [newCategory, setNewCategory] = useState("");
  const [difficulty, setDifficulty] = useState("");
  const [newDifficulty, setNewDifficulty] = useState("");
  const [timeLimit, setTimeLimit] = useState(10);
  const [message, setMessage] = useState("");

  useEffect(() => {
    const savedCategories = JSON.parse(localStorage.getItem("quizCategories")) || ["Istorija", "Programiranje", "MySQL"];
    const savedDifficulties = JSON.parse(localStorage.getItem("quizDifficulties")) || ["Lako", "Srednje", "Teško"];
    setCategories(savedCategories);
    setDifficulties(savedDifficulties);
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();

    const finalCategory = category === "new" ? newCategory : category;
    const finalDifficulty = difficulty === "new" ? newDifficulty : difficulty;

    if (finalCategory && !categories.includes(finalCategory)) {
      const updatedCategories = [...categories, finalCategory];
      setCategories(updatedCategories);
      localStorage.setItem("quizCategories", JSON.stringify(updatedCategories));
    }

    if (finalDifficulty && !difficulties.includes(finalDifficulty)) {
      const updatedDifficulties = [...difficulties, finalDifficulty];
      setDifficulties(updatedDifficulties);
      localStorage.setItem("quizDifficulties", JSON.stringify(updatedDifficulties));
    }

    try {
      await createQuiz({ title, description, category: finalCategory, difficultyLevel: finalDifficulty, timeLimit });
      alert("Quiz created successfully!");
      navigate("/quizzes");
    } catch (err) {
      console.error(err);
      setMessage("Failed to create quiz.");
    }
  };

  return (
    <div className="create-quiz-container">
      <h2 className="create-quiz-title">Create Quiz</h2>
      <form className="create-quiz-form" onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          required
        />
        <textarea
          placeholder="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          required
        />

        <select value={category} onChange={(e) => setCategory(e.target.value)} required>
          <option value="">Select category</option>
          {categories.map((cat) => (
            <option key={cat} value={cat}>{cat}</option>
          ))}
          <option value="new">New category...</option>
        </select>
        {category === "new" && (
          <input
            type="text"
            placeholder="Enter new category"
            value={newCategory}
            onChange={(e) => setNewCategory(e.target.value)}
            required
          />
        )}

        <select value={difficulty} onChange={(e) => setDifficulty(e.target.value)} required>
          <option value="">Select difficulty</option>
          {difficulties.map((dif) => (
            <option key={dif} value={dif}>{dif}</option>
          ))}
          <option value="new">New difficulty...</option>
        </select>
        {difficulty === "new" && (
          <input
            type="text"
            placeholder="Enter new difficulty"
            value={newDifficulty}
            onChange={(e) => setNewDifficulty(e.target.value)}
            required
          />
        )}

        <input
          type="number"
          placeholder="Time Limit (minutes)"
          value={timeLimit}
          onChange={(e) => setTimeLimit(Number(e.target.value))}
          required
        />

        <button type="submit">Create Quiz</button>
      </form>
      {message && <p className="create-quiz-message">{message}</p>}
    </div>
  );
};

export default CreateQuiz;
