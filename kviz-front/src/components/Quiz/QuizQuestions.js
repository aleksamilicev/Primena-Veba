import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { getQuizById, getQuestionsByQuizId, deleteQuestionById } from "../../api/services/quizService";
import { useAuth } from "../../context/AuthContext";

const QuizQuestions = () => {
  const { quizId } = useParams();
  const [quiz, setQuiz] = useState(null);
  const [questions, setQuestions] = useState([]);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();

  useEffect(() => {
    fetchQuizAndQuestions();
  }, []);

  const fetchQuizAndQuestions = async () => {
    try {
      setLoading(true);
      const quizData = await getQuizById(quizId);
      setQuiz(quizData);
      
      const questionsData = await getQuestionsByQuizId(quizId);
      console.log("Questions data:", questionsData); // za debug
      setQuestions(questionsData);
    } catch (error) {
      console.error("Error fetching quiz/questions:", error);
      alert("Greška pri učitavanju podataka");
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteQuestion = async (questionId) => {
    if (!window.confirm("Da li ste sigurni da želite da obrišete ovo pitanje?")) {
      return;
    }

    try {
      await deleteQuestionById(questionId);
      // Ukloni pitanje iz state-a
      setQuestions(questions.filter((question) => question.QuestionId !== questionId));
      alert("Pitanje je uspešno obrisano!");
    } catch (error) {
      console.error("Failed to delete question:", error);
      alert("Greška pri brisanju pitanja.");
    }
  };

  if (loading) return <div>Učitavanje...</div>;

  // Proveri da li je korisnik admin
  if (!user?.isAdmin) {
    return <div>Nemate dozvolu za pristup ovoj stranici.</div>;
  }

  return (
    <div style={{ padding: "20px" }}>
      <h1>Pitanja za kviz: {quiz?.Title || quiz?.title}</h1>
      
      <div style={{ marginBottom: "20px" }}>
        <Link to="/quizzes" style={{ marginRight: "15px", color: "#007bff", textDecoration: "none" }}>
          ← Nazad na kvizove
        </Link>
        <Link 
          to={`/admin/quizzes/${quizId}/add-question`} 
          style={{ 
            padding: "8px 16px", 
            backgroundColor: "#28a745", 
            color: "white", 
            textDecoration: "none", 
            borderRadius: "4px" 
          }}
        >
          + Dodaj novo pitanje
        </Link>
      </div>

      {questions.length === 0 ? (
        <p>Nema pitanja za ovaj kviz.</p>
      ) : (
        <table style={{ width: "100%", borderCollapse: "collapse", border: "1px solid #ddd" }}>
          <thead>
            <tr style={{ backgroundColor: "#f8f9fa" }}>
              <th style={{ padding: "12px", border: "1px solid #ddd", textAlign: "left" }}>Tekst pitanja</th>
              <th style={{ padding: "12px", border: "1px solid #ddd", textAlign: "left" }}>Tip</th>
              <th style={{ padding: "12px", border: "1px solid #ddd", textAlign: "left" }}>Težina</th>
              <th style={{ padding: "12px", border: "1px solid #ddd", textAlign: "left" }}>Tačan odgovor</th>
              <th style={{ padding: "12px", border: "1px solid #ddd", textAlign: "center" }}>Akcije</th>
            </tr>
          </thead>
          <tbody>
            {questions.map((q) => (
              <tr key={q.QuestionId}>
                <td style={{ padding: "12px", border: "1px solid #ddd" }}>
                  {q.QuestionText || "N/A"}
                </td>
                <td style={{ padding: "12px", border: "1px solid #ddd" }}>
                  {q.QuestionType || "N/A"}
                </td>
                <td style={{ padding: "12px", border: "1px solid #ddd" }}>
                  {q.DifficultyLevel || "N/A"}
                </td>
                <td style={{ padding: "12px", border: "1px solid #ddd" }}>
                  {q.CorrectAnswer || "N/A"}
                </td>
                <td style={{ padding: "12px", border: "1px solid #ddd", textAlign: "center" }}>
                  <Link
                    to={`/admin/questions/${q.QuestionId}/edit`}
                    style={{
                      marginRight: "10px",
                      color: "#ffc107",
                      textDecoration: "none",
                      fontWeight: "bold",
                      padding: "4px 8px",
                      border: "1px solid #ffc107",
                      borderRadius: "3px"
                    }}
                  >
                    Edit
                  </Link>
                  <button
                    onClick={() => handleDeleteQuestion(q.QuestionId)}
                    style={{
                      color: "white",
                      backgroundColor: "#dc3545",
                      border: "1px solid #dc3545",
                      padding: "4px 8px",
                      borderRadius: "3px",
                      cursor: "pointer",
                      fontWeight: "bold"
                    }}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

export default QuizQuestions;