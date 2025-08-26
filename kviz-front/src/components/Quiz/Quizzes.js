import React, { useEffect, useState } from "react";
import { getQuizzes } from "../../api/services/quizService";
import QuizCard from "./QuizCard";
import QuizFilter from "./QuizFilter";

const Quizzes = () => {
  const [quizzes, setQuizzes] = useState([]);
  const [filteredQuizzes, setFilteredQuizzes] = useState([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const data = await getQuizzes();
        setQuizzes(data);
        setFilteredQuizzes(data);
      } catch (error) {
        console.error("Failed to fetch quizzes:", error);
      }
    };
    fetchData();
  }, []);

  const handleFilter = (category, difficulty) => {
    let filtered = quizzes;
    if (category) filtered = filtered.filter((q) => q.Category === category);
    if (difficulty) filtered = filtered.filter((q) => q.Difficulty_Level === difficulty);
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
