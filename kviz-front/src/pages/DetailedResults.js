import { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import { fetchDetailedResult } from "../api/services/resultService";
import { ResultDetail } from "../api/models/ResultDetail";
import PerformancePieChart from "../components/Results/Charts/PerformancePieChart";
import ProgressLineChart from "../components/Results/Charts/ProgressLineChart";
import QuestionBreakdown from "../components/Results/QuestionBreakdown";
import "../styles/DetailedResults.css";

export default function DetailedResults() {
  const { resultId } = useParams();
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadResult = async () => {
      try {
        const data = await fetchDetailedResult(resultId);
        setResult(new ResultDetail(data));
      } catch (err) {
        console.error("Failed to fetch result:", err);
      } finally {
        setLoading(false);
      }
    };
    loadResult();
  }, [resultId]);

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

  const getScorePercentage = () => {
    return Math.round((result.correctAnswers / result.totalQuestions) * 100);
  };

  const getScoreClass = (percentage) => {
    if (percentage >= 90) return 'excellent';
    if (percentage >= 70) return 'good';
    if (percentage >= 50) return 'average';
    return 'poor';
  };

  if (loading) {
    return (
      <div className="detailed-results-container">
        <div className="loading-state">
          <div className="loading-spinner"></div>
          <p>Učitavanje detaljnih rezultata...</p>
        </div>
      </div>
    );
  }

  if (!result) {
    return (
      <div className="detailed-results-container">
        <div className="error-state">
          <p>Rezultat nije pronađen</p>
          <Link to="/my-results" className="back-btn">
            Nazad na rezultate
          </Link>
        </div>
      </div>
    );
  }

  const scorePercentage = getScorePercentage();

  return (
    <div className="detailed-results-container">
      <div className="header-section">
        <div className="title-area">
          <h1 className="page-title">{result.quizTitle}</h1>
          <p className="page-subtitle">Detaljni rezultati kviza</p>
        </div>

        <div className="summary-card">
          <div className="score-display">
            <div className={`score-circle ${getScoreClass(scorePercentage)}`}>
              <span className="score-percentage">{scorePercentage}%</span>
              <span className="score-fraction">{result.correctAnswers}/{result.totalQuestions}</span>
            </div>
          </div>
          
          <div className="summary-stats">
            <div className="stat-item">
              <span className="stat-icon">📅</span>
              <div className="stat-content">
                <span className="stat-label">Datum</span>
                <span className="stat-value">{formatDate(result.completedAt)}</span>
              </div>
            </div>
            
            <div className="stat-item">
              <span className="stat-icon">⏱</span>
              <div className="stat-content">
                <span className="stat-label">Vreme</span>
                <span className="stat-value">{formatTime(result.timeTaken)}</span>
              </div>
            </div>
          </div>

          <div className="pie-chart-container">
            <PerformancePieChart
              correct={result.correctAnswers}
              wrong={result.totalQuestions - result.correctAnswers}
            />
          </div>
        </div>
      </div>

      <div className="content-grid">
        {result.attemptsHistory.length > 1 && (
          <div className="chart-card full-width">
            <div className="card-header">
              <h3 className="card-title">Napredak tokom vremena</h3>
            </div>
            <div className="card-content">
              <ProgressLineChart history={result.attemptsHistory} />
            </div>
          </div>
        )}

        <div className="breakdown-card">
          <div className="card-header">
            <h3 className="card-title">Pregled pitanja</h3>
          </div>
          <div className="card-content">
            <QuestionBreakdown answers={result.answers} />
          </div>
        </div>
      </div>

      <div className="actions-section">
        <Link to="/my-results" className="back-btn">
          <span className="btn-icon">←</span>
          Nazad na rezultate
        </Link>
      </div>
    </div>
  );
}