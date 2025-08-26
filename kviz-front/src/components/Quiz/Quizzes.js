import React, { useEffect, useState } from "react";
import { getQuizzes } from "../../api/services/quizService";
import QuizCard from "./QuizCard";
import QuizFilter from "./QuizFilter";

const Quizzes = () => {
  const [quizzes, setQuizzes] = useState([]);
  const [filteredQuizzes, setFilteredQuizzes] = useState([]);

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
      <h1>Available Quizzes</h1>
      <QuizFilter quizzes={quizzes} onFilter={handleFilter} />
      <div className="quiz-list">
        {filteredQuizzes.map((quiz) => (
          <QuizCard key={quiz.Quiz_Id} quiz={quiz} />
        ))}
      </div>
    </div>
  );
};

export default Quizzes;
