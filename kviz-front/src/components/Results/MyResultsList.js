import { useNavigate } from "react-router-dom";
import "../../styles/MyResultsList.css";

export default function MyResultsList({ results }) {
  const navigate = useNavigate();

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return mins > 0 ? `${mins}m ${secs}s` : `${secs}s`;
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('sr-RS', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getScoreClass = (percentage) => {
    if (percentage >= 90) return 'excellent';
    if (percentage >= 70) return 'good';
    if (percentage >= 50) return 'average';
    return 'poor';
  };

  return (
    <div className="results-list">
      {results.map((res) => (
        <div key={res.resultId} className="result-card">
          <div className="result-header">
            <h3 className="quiz-title">{res.quizTitle}</h3>
            <span className="completion-date">{formatDate(res.completedAt)}</span>
          </div>
          
          <div className="result-stats">
            <div className="stats-row">
              <div className={`score-badge ${getScoreClass(res.scorePercentage)}`}>
                {Math.round(res.scorePercentage)}%
              </div>
              
              <div className="stats-group">
                <span className="stat-item">
                  <span className="stat-icon">✅</span>
                  {res.correctAnswers}/{res.totalQuestions}
                </span>
                <span className="stat-item">
                  <span className="stat-icon">⏱</span>
                  {formatTime(res.timeTaken)}
                </span>
              </div>
            </div>
          </div>

          <button
            className="details-btn"
            onClick={() =>
              navigate(`/detailed-results/${res.resultId}`, {
                state: { quizId: res.quizId, userId: res.userId },
              })
            }
          >
            <span className="btn-icon">🔍</span>
            Detalji
          </button>
        </div>
      ))}
    </div>
  );
}