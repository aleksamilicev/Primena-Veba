import { useState } from "react";
import { createQuiz } from "../../api/services/quizService";
import Quiz from "../../api/models/Quiz";
import { useNavigate } from "react-router-dom";

function CreateQuiz() {
  const navigate = useNavigate();
  const [quiz, setQuiz] = useState(new Quiz());

  const handleChange = (e) => {
    const { name, value } = e.target;
    setQuiz((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    await createQuiz(quiz);
    navigate("/admin/quizzes");
  };

  return (
    <div>
      <h1>Create Quiz</h1>
      <form onSubmit={handleSubmit}>
        <input name="title" placeholder="Title" onChange={handleChange} />
        <textarea name="description" placeholder="Description" onChange={handleChange} />
        <input name="difficultyLevel" placeholder="Difficulty" onChange={handleChange} />
        <input name="category" placeholder="Category" onChange={handleChange} />
        <input type="number" name="timeLimit" placeholder="Time Limit" onChange={handleChange} />
        <button type="submit">Create</button>
      </form>
    </div>
  );
}

export default CreateQuiz;
