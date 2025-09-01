import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { fetchRankings } from "../api/services/rankingService";
import "../styles/Ranking.css"; // dodajte ovaj CSS fajl

export default function Ranking() {
  const { user } = useAuth();
  const [rankings, setRankings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [period, setPeriod] = useState("all");
  const [quizId, setQuizId] = useState("");

  const loadRankings = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await fetchRankings(period, quizId);
      setRankings(data);
    } catch {
      setError("Failed to load rankings");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRankings();
  }, [period, quizId]);

  const getRankIcon = (position) => {
    if (position === 1) return "🥇";
    if (position === 2) return "🥈";
    if (position === 3) return "🥉";
    return `#${position}`;
  };

  const getRankClass = (position) => {
    if (position === 1) return "gold";
    if (position === 2) return "silver";
    if (position === 3) return "bronze";
    return "regular";
  };

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return mins > 0 ? `${mins}m ${secs}s` : `${secs}s`;
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInDays = Math.floor((now - date) / (1000 * 60 * 60 * 24));
    
    if (diffInDays === 0) return 'Danas';
    if (diffInDays === 1) return 'Juče';
    if (diffInDays < 7) return `Pre ${diffInDays} dana`;
    
    return date.toLocaleDateString('sr-RS', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const isCurrentUser = (username) => {
    return user?.username === username;
  };

  return (
    <div className="ranking-container">
      <div className="ranking-header">
        <h2 className="ranking-title">🏆 Globalni ranking</h2>
        <p className="ranking-subtitle">Najbolji rezultati svih korisnika</p>
      </div>

      <div className="controls-section">
        <div className="period-controls">
          {["week", "month", "all"].map((p) => (
            <button
              key={p}
              className={`period-btn ${period === p ? "active" : ""}`}
              onClick={() => setPeriod(p)}
            >
              {p === "week" ? "📅 Ova nedelja" : 
               p === "month" ? "📆 Ovaj mesec" : 
               "⏳ Sva vremena"}
            </button>
          ))}
        </div>

        <div className="quiz-filter">
          <label className="filter-label">🔍 Filter po kvizu:</label>
          <div className="filter-input-group">
            <input
              type="number"
              min="1"
              placeholder="Quiz ID"
              value={quizId}
              onChange={(e) => setQuizId(e.target.value)}
              className="quiz-input"
            />
            <button
              onClick={loadRankings}
              className="apply-btn"
            >
              ✓
            </button>
          </div>
        </div>
      </div>

      <div className="ranking-content">
        {loading && (
          <div className="loading-state">
            <div className="loading-spinner"></div>
            <p>Učitavanje rangiranja...</p>
          </div>
        )}

        {error && (
          <div className="error-state">
            <div className="error-icon">⚠️</div>
            <p>Greška pri učitavanju rangiranja</p>
            <button onClick={loadRankings} className="retry-btn">
              🔄 Pokušaj ponovo
            </button>
          </div>
        )}

        {!loading && !error && rankings.length === 0 && (
          <div className="empty-state">
            <div className="empty-icon">🏁</div>
            <h3>Nema rangiranja</h3>
            <p>Trenutno nema dostupnih rezultata za prikazane filtere.</p>
          </div>
        )}

        {!loading && rankings.length > 0 && (
          <div className="rankings-list">
            {rankings.map((r, idx) => (
              <div
                key={`${r.quizId}-${r.username}-${idx}`}
                className={`ranking-item ${getRankClass(r.rankPosition)} ${
                  isCurrentUser(r.username) ? "current-user" : ""
                }`}
              >
                <div className="rank-position">
                  <span className="rank-icon">{getRankIcon(r.rankPosition)}</span>
                </div>
                
                <div className="user-info">
                  <div className="username">
                    {r.username}
                    {isCurrentUser(r.username) && (
                      <span className="you-badge">Vi</span>
                    )}
                  </div>
                  <div className="quiz-id">Kviz #{r.quizId ?? quizId}</div>
                </div>

                <div className="performance-stats">
                  <div className="score-section">
                    <div className="score-value">{r.scorePercentage}%</div>
                    <div className="score-label">rezultat</div>
                  </div>
                  
                  <div className="time-section">
                    <div className="time-value">{formatTime(r.timeTaken)}</div>
                    <div className="time-label">vreme</div>
                  </div>
                  
                  <div className="date-section">
                    <div className="date-value">{formatDate(r.completedAt)}</div>
                    <div className="date-label">završeno</div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}