import React, { useEffect, useState } from "react";
import { getQuizzes } from "../../api/services/quizService";
import QuizCard from "./QuizCard";
import QuizFilter from "./QuizFilter";
import ProfileDropdown from "../Layout/ProfileDropdown";
import { useAuth } from "../../context/AuthContext";
import { Link } from "react-router-dom";

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
    <div>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <h1>Available Quizzes</h1>
        <ProfileDropdown />
      </div>
      
      {/* Ako je admin, prikaži dugme za kreiranje kviza */}
      {user?.isAdmin && (
        <div style={{ margin: "10px 0" }}>
          <Link 
            to="/admin/quizzes/create" 
            style={{ 
              padding: "10px 20px", 
              backgroundColor: "#007bff", 
              color: "white", 
              textDecoration: "none", 
              borderRadius: "5px" 
            }}
          >
            + Create Quiz
          </Link>
        </div>
      )}
      
      <QuizFilter quizzes={quizzes} onFilter={handleFilter} />
      
      <div className="quiz-list">
        {filteredQuizzes.map((quiz) => (
          <div key={quiz.Quiz_Id}>
            <QuizCard quiz={quiz} />
            {/* Ako je admin, prikaži link za dodavanje pitanja */}
            {user?.isAdmin && (
              <Link
                to={`/admin/quizzes/${quiz.Quiz_Id}/add-question`}
                style={{ 
                  display: "block", 
                  marginTop: "5px", 
                  color: "#28a745",
                  textDecoration: "none",
                  fontSize: "14px",
                  fontWeight: "bold"
                }}
              >
                + Add Question to this Quiz
              </Link>
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export default Quizzes;