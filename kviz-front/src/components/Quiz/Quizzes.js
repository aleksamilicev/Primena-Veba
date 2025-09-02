import React, { useEffect, useState } from "react";
import { getQuizzes, deleteQuizById } from "../../api/services/quizService";
import QuizCard from "./QuizCard";
import QuizFilter from "./QuizFilter";
import ProfileDropdown from "../Layout/ProfileDropdown";
import { useAuth } from "../../context/AuthContext";
import { Link } from "react-router-dom";
import "../../styles/Quizzes.css"; 

const Quizzes = () => {
  const [quizzes, setQuizzes] = useState([]);
  const [filteredQuizzes, setFilteredQuizzes] = useState([]);
  const { user } = useAuth();

  useEffect(() => {
    fetchQuizzes();
  }, []);

  const fetchQuizzes = async (search = "") => {
    try {
      const data = await getQuizzes(search);
      setQuizzes(data);
      setFilteredQuizzes(data);
    } catch (error) {
      console.error("Failed to fetch quizzes:", error);
    }
  };

  const handleFilter = (category, difficulty, search) => {
    let filtered = quizzes;
    if (category) filtered = filtered.filter((q) => q.Category === category);
    if (difficulty) filtered = filtered.filter((q) => q.Difficulty_Level === difficulty);
    if (search) {
      const term = search.toLowerCase();
      filtered = filtered.filter(
        (q) =>
          (q.Title && q.Title.toLowerCase().includes(term)) ||
          (q.Description && q.Description.toLowerCase().includes(term))
      );
    }
    setFilteredQuizzes(filtered);
  };

  return (
    <div className="quizzes-container">
      <div className="quizzes-header">
        <h1>Available Quizzes</h1>
        <ProfileDropdown />
      </div>

      


      {user?.isAdmin && (
        <div className="create-quiz-btn">
          <Link to="/admin/quizzes/create">+ Create Quiz</Link>
        </div>
      )}

      <QuizFilter quizzes={quizzes} onFilter={handleFilter} />

      <div className="nav-links">
        <Link to="/my-results">My Results</Link>
        <Link to="/ranking">Ranking</Link>
        <Link to="/">Home</Link>
        {user?.isAdmin && <Link to="/all-results">All Results</Link>}
      </div>

      <div className="quiz-list">
        {filteredQuizzes.map((quiz) => (
          <div key={quiz.Quiz_Id} className="quiz-wrapper">
            <QuizCard quiz={quiz} />

            {user?.isAdmin && (
              <div className="admin-actions">
                <Link to={`/admin/quizzes/${quiz.Quiz_Id}/add-question`} className="add-question">
                  + Add Question
                </Link>
                <Link to={`/admin/quizzes/${quiz.Quiz_Id}/questions`} className="view-questions">
                  View Questions
                </Link>
                <Link to={`/admin/quizzes/${quiz.Quiz_Id}/edit`} className="edit-quiz">
                  Edit Quiz
                </Link>
                <button
                  className="delete-quiz"
                  onClick={async () => {
                    if (window.confirm("Are you sure you want to delete this quiz?")) {
                      try {
                        await deleteQuizById(quiz.Quiz_Id);
                        setQuizzes(quizzes.filter((q) => q.Quiz_Id !== quiz.Quiz_Id));
                        setFilteredQuizzes(filteredQuizzes.filter((q) => q.Quiz_Id !== quiz.Quiz_Id));
                      } catch (error) {
                        console.error("Failed to delete quiz:", error);
                        alert("Failed to delete quiz.");
                      }
                    }
                  }}
                >
                  Delete Quiz
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default Quizzes;
