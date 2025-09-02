// src/components/Quiz/QuizFilter.js
import React, { useState } from "react";
import "../../styles/QuizFilter.css";

const QuizFilter = ({ quizzes, onFilter }) => {
  const [category, setCategory] = useState("");
  const [difficulty, setDifficulty] = useState("");
  const [search, setSearch] = useState("");

  const categories = [...new Set(quizzes.map((q) => q.Category))];
  const difficulties = [...new Set(quizzes.map((q) => q.Difficulty_Level))];

  const applyFilter = () => {
    onFilter(category, difficulty, search);
  };

  return (
    <div className="quiz-filter">
      <input
        type="text"
        placeholder="🔍 Search..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      <select value={category} onChange={(e) => setCategory(e.target.value)}>
        <option value="">All Categories</option>
        {categories.map((cat, i) => (
          <option key={i} value={cat}>
            {cat}
          </option>
        ))}
      </select>

      <select
        value={difficulty}
        onChange={(e) => setDifficulty(e.target.value)}
      >
        <option value="">All Difficulties</option>
        {difficulties.map((diff, i) => (
          <option key={i} value={diff}>
            {diff}
          </option>
        ))}
      </select>

      <button onClick={applyFilter}>Apply Filter</button>
    </div>
  );
};

export default QuizFilter;
