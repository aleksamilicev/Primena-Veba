// src/components/Quiz/QuizFilter.js
import React, { useState } from "react";

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
    <div style={{ marginBottom: "20px" }}>
      <input
        type="text"
        placeholder="Search..."
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        style={{ marginRight: "10px" }}
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
        style={{ marginLeft: "10px" }}
      >
        <option value="">All Difficulties</option>
        {difficulties.map((diff, i) => (
          <option key={i} value={diff}>
            {diff}
          </option>
        ))}
      </select>

      <button onClick={applyFilter} style={{ marginLeft: "10px" }}>
        Apply Filter
      </button>
    </div>
  );
};

export default QuizFilter; // ← default export
