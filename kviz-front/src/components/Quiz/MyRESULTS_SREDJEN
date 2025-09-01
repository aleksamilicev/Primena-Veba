import React, { useEffect, useState } from "react";
import "../../styles/MyResults.css"; // dodajte ovaj CSS fajl

const MyResults = () => {
  const [results, setResults] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [error, setError] = useState(null);
  const [sortBy, setSortBy] = useState('date'); // date, score, time
  const [filterBy, setFilterBy] = useState('all'); // all, excellent, good, average, poor
  const [searchTerm, setSearchTerm] = useState('');

  const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

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

  const getScoreCategory = (percentage) => {
    if (percentage >= 90) return 'excellent';
    if (percentage >= 70) return 'good';
    if (percentage >= 50) return 'average';
    return 'poor';
  };

  const getScoreIcon = (percentage) => {
    if (percentage >= 90) return '🏆';
    if (percentage >= 70) return '🎯';
    if (percentage >= 50) return '📈';
    return '📉';
  };

  const sortedAndFilteredResults = [...results]
    .filter(result => {
      // Filter by search term
      const matchesSearch = result.quizTitle?.toLowerCase().includes(searchTerm.toLowerCase());
      
      // Filter by score category
      if (filterBy === 'all') return matchesSearch;
      return matchesSearch && getScoreCategory(result.scorePercentage) === filterBy;
    })
    .sort((a, b) => {
      switch (sortBy) {
        case 'score':
          return b.scorePercentage - a.scorePercentage;
        case 'time':
          return a.timeTaken - b.timeTaken;
        case 'date':
        default:
          return new Date(b.completedAt) - new Date(a.completedAt);
      }
    });

  const getAverageScore = () => {
    if (results.length === 0) return 0;
    const total = results.reduce((sum, result) => sum + result.scorePercentage, 0);
    return Math.round(total / results.length);
  };

  const getBestScore = () => {
    if (results.length === 0) return 0;
    return Math.max(...results.map(result => result.scorePercentage));
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
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-content">
          <div className="loading-spinner"></div>
          <h3>Učitavanje rezultata...</h3>
          <p>Molimo sačekajte dok učitavamo vaše rezultate</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="error-container">
        <div className="error-content">
          <div className="error-icon">⚠️</div>
          <h3>Ups! Nešto je pošlo po zlu</h3>
          <p>{error}</p>
          <button 
            onClick={() => fetchResults(page)} 
            className="retry-button"
          >
            🔄 Pokušaj ponovo
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="my-results-container">
      <div className="results-header">
        <div className="header-content">
          <h1 className="page-title">📊 Moji rezultati kvizova</h1>
          <p className="page-subtitle">Pregled vaših dosadašnjih pokušaja</p>
        </div>
        
        {results.length > 0 && (
          <div className="stats-cards">
            <div className="stat-card">
              <div className="stat-icon">📈</div>
              <div className="stat-content">
                <span className="stat-value">{getAverageScore()}%</span>
                <span className="stat-label">Prosečan rezultat</span>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">🏆</div>
              <div className="stat-content">
                <span className="stat-value">{getBestScore()}%</span>
                <span className="stat-label">Najbolji rezultat</span>
              </div>
            </div>
            <div className="stat-card">
              <div className="stat-icon">📝</div>
              <div className="stat-content">
                <span className="stat-value">{results.length}</span>
                <span className="stat-label">Ukupno kvizova</span>
              </div>
            </div>
          </div>
        )}
      </div>

      {results.length > 0 && (
        <div className="controls-section">
          <div className="search-container">
            <input
              type="text"
              placeholder="🔍 Pretraži kvizove..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input"
            />
          </div>
          
          <div className="filters-container">
            <div className="filter-group">
              <label>Sortiraj po:</label>
              <select 
                value={sortBy} 
                onChange={(e) => setSortBy(e.target.value)}
                className="filter-select"
              >
                <option value="date">📅 Datum</option>
                <option value="score">🎯 Rezultat</option>
                <option value="time">⏱️ Vreme</option>
              </select>
            </div>
            
            <div className="filter-group">
              <label>Filtriraj po:</label>
              <select 
                value={filterBy} 
                onChange={(e) => setFilterBy(e.target.value)}
                className="filter-select"
              >
                <option value="all">🔍 Svi rezultati</option>
                <option value="excellent">🏆 Odlični (90%+)</option>
                <option value="good">🎯 Dobri (70-89%)</option>
                <option value="average">📈 Prosečni (50-69%)</option>
                <option value="poor">📉 Slabi (&lt;50%)</option>
              </select>
            </div>
          </div>
        </div>
      )}

      {sortedAndFilteredResults.length === 0 ? (
        <div className="no-results">
          <div className="no-results-icon">📋</div>
          <h3>
            {searchTerm || filterBy !== 'all' 
              ? 'Nema rezultata koji odgovaraju vašoj pretrazi' 
              : 'Još uvek nemate rezultate kvizova'
            }
          </h3>
          <p>
            {searchTerm || filterBy !== 'all'
              ? 'Pokušajte sa drugačijim pretragom ili filtrom.'
              : 'Rešite neki kviz da biste videli vaše rezultate ovde!'
            }
          </p>
        </div>
      ) : (
        <div className="results-grid">
          {sortedAndFilteredResults.map((result) => (
            <div key={result.resultId} className="result-card">
              <div className="card-header">
                <div className="quiz-info">
                  <h3 className="quiz-title">{result.quizTitle}</h3>
                  <span className="attempt-id">Pokušaj #{result.attemptId}</span>
                </div>
                <div className="date-badge">
                  {formatDate(result.completedAt)}
                </div>
              </div>
              
              <div className="card-content">
                <div className="score-section">
                  <div className={`score-circle ${getScoreCategory(result.scorePercentage)}`}>
                    <div className="score-percentage">
                      {Math.round(result.scorePercentage)}%
                    </div>
                    <div className="score-icon">
                      {getScoreIcon(result.scorePercentage)}
                    </div>
                  </div>
                  
                  <div className="score-details">
                    <div className="correct-answers">
                      <span className="label">Tačni odgovori:</span>
                      <span className="value">
                        {result.correctAnswers}/{result.totalQuestions}
                      </span>
                    </div>
                    <div className="time-taken">
                      <span className="label">Vreme:</span>
                      <span className="value">{formatTime(result.timeTaken)}</span>
                    </div>
                  </div>
                </div>
                
                <div className="card-footer">
                  <div className="completion-date">
                    📅 {new Date(result.completedAt).toLocaleString('sr-RS', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {pagination && pagination.totalPages > 1 && (
        <div className="pagination-container">
          <div className="pagination">
            <button 
              disabled={page === 1} 
              onClick={() => setPage(p => p - 1)}
              className="pagination-button prev"
            >
              ⬅️ Prethodna
            </button>
            
            <div className="pagination-info">
              <span className="current-page">{pagination.currentPage}</span>
              <span className="page-separator">od</span>
              <span className="total-pages">{pagination.totalPages}</span>
            </div>
            
            <button
              disabled={page === pagination.totalPages}
              onClick={() => setPage(p => p + 1)}
              className="pagination-button next"
            >
              Sledeća ➡️
            </button>
          </div>
          
          <div className="pagination-details">
            Prikazano {sortedAndFilteredResults.length} od {pagination.totalItems} rezultata
          </div>
        </div>
      )}
    </div>
  );
};

export default MyResults;