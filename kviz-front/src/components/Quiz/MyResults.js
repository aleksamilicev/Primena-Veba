import React, { useEffect, useState } from "react";

const MyResults = () => {
  const [results, setResults] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [error, setError] = useState(null);

  const API_BASE_URL = process.env.REACT_APP_API_BASE_URL; // 👈 isto kao u QuizTaking

  const apiCall = async (url, method = "GET") => {
    const token = localStorage.getItem("token");

    const response = await fetch(`${API_BASE_URL}${url}`, {
      method,
      headers: {
        "Content-Type": "application/json",
        Authorization: token ? `Bearer ${token}` : "",
      },
    });

    if (!response.ok) {
      const errorData = await response.text();
      throw new Error(errorData || `HTTP error! status: ${response.status}`);
    }

    return response.json();
  };

  useEffect(() => {
    fetchResults(page);
  }, [page]);

  const fetchResults = async (page) => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiCall(`/quizresults/my-results?page=${page}&pageSize=10`);
      setResults(data.results || []);
      setPagination(data.pagination || null);
    } catch (err) {
      console.error("❌ Greška pri dohvatanju rezultata:", err.message);
      setError(err.message);
      setResults([]);
      setPagination(null);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ padding: "20px" }}>
      <h1>📊 My Quiz Results</h1>

      {loading ? (
        <p>Loading...</p>
      ) : error ? (
        <p style={{ color: "red" }}>Greška: {error}</p>
      ) : results.length === 0 ? (
        <p>Nema rezultata za prikaz.</p>
      ) : (
        <div>
          {results.map((res) => (
            <div
              key={res.resultId}
              style={{
                border: "1px solid #ccc",
                borderRadius: "6px",
                padding: "10px",
                marginBottom: "10px",
              }}
            >
              <h3>{res.quizTitle}</h3>
              <p>
                Attempt ID: {res.attemptId} | Date:{" "}
                {new Date(res.completedAt).toLocaleString()}
              </p>
              <p>
                ✅ {res.correctAnswers}/{res.totalQuestions} correct | 📊{" "}
                {Math.round(res.scorePercentage)}% | ⏱ {res.timeTaken}s
              </p>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {pagination && pagination.totalPages > 1 && (
        <div style={{ marginTop: "20px" }}>
          <button disabled={page === 1} onClick={() => setPage((p) => p - 1)}>
            ⬅ Prev
          </button>
          <span style={{ margin: "0 10px" }}>
            Page {pagination.currentPage} of {pagination.totalPages}
          </span>
          <button
            disabled={page === pagination.totalPages}
            onClick={() => setPage((p) => p + 1)}
          >
            Next ➡
          </button>
        </div>
      )}
    </div>
  );
};

export default MyResults;
