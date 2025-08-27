import { useEffect, useState } from "react";
import { getAllQuizzes, deleteQuiz } from "../../api/services/quizService";
import { Link } from "react-router-dom";

function AdminQuizList() {
  const [quizzes, setQuizzes] = useState([]);

  useEffect(() => {
    loadQuizzes();
  }, []);

  const loadQuizzes = async () => {
    const data = await getAllQuizzes();
    setQuizzes(data);
  };

  const handleDelete = async (id) => {
    await deleteQuiz(id);
    loadQuizzes();
  };

  return (
    <div>
      <h1>Manage Quizzes</h1>
      {quizzes.map((quiz) => (
        <div key={quiz.id}>
          <h3>{quiz.title}</h3>
          <Link to={`/admin/quizzes/${quiz.id}`}>View</Link>
          <Link to={`/admin/quizzes/${quiz.id}/edit`}>Edit</Link>
          <Link to={`/admin/quizzes/${quiz.id}/create-question`}>Add Question</Link>
          <button onClick={() => handleDelete(quiz.id)}>Delete</button>
        </div>
      ))}
    </div>
  );
}

export default AdminQuizList;
