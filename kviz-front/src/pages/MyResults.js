import { useEffect, useState } from "react";
import { fetchMyResults } from "../api/services/resultService";
import { Result } from "../api/models/Result";
import MyResultsList from "../components/Results/MyResultsList";
import Pagination from "../components/Results/Pagination";
import { Link } from "react-router-dom";
import "../styles/MyResults.css";

export default function MyResults() {
  const [results, setResults] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [sortBy, setSortBy] = useState('date');
  const [searchTerm, setSearchTerm] = useState('');

  const pageSize = 10;

  const loadResults = async (page) => {
    try {
      setLoading(true);
      setError(null);
      const data = await fetchMyResults(page, pageSize);
      setResults(data.results.map((r) => new Result(r)));
      setPagination(data.pagination);
    } catch (err) {
      console.error("❌ Greška pri dohvatanju rezultata:", err.message);
      setError("Failed to load results");
      setResults([]);
      setPagination(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadResults(page);
  }, [page]);

  // Sortiranje i filtriranje
  const processedResults = [...results]
    .filter(r => r.quizTitle?.toLowerCase().includes(searchTerm.toLowerCase()))
    .sort((a, b) => {
      switch (sortBy) {
        case 'score': return b.scorePercentage - a.scorePercentage;
        case 'time': return a.timeTaken - b.timeTaken;
        case 'date':
        default: return new Date(b.completedAt) - new Date(a.completedAt);
      }
    });

  const getAverageScore = () => results.length === 0 ? 0 : Math.round(results.reduce((sum, r) => sum + r.scorePercentage, 0) / results.length);
  const getBestScore = () => results.length === 0 ? 0 : Math.max(...results.map(r => r.scorePercentage));

  if (loading) return (
    <div className="my-results-container">
      <div className="loading-state">
        <div className="loading-dot"></div>
        <p>Učitavanje rezultata...</p>
      </div>
    </div>
  );

  if (error) return (
    <div className="my-results-container">
      <div className="error-state">
        <p>Greška pri učitavanju rezultata</p>
        <button onClick={() => loadResults(page)} className="retry-btn">
          Pokušaj ponovo
        </button>
      </div>
    </div>
  );

  return (
    <div className="my-results-container">
      <div className="header-section">
        <div className="nav-links">
          <Link to="/">Home</Link>
          <Link to="/quizzes">Kvizovi</Link>
        </div>
        <h2 className="page-title">Moji rezultati</h2>
        {results.length > 0 && (
          <div className="stats-summary">
            <div className="stat-item">
              <span className="stat-value">{getAverageScore()}%</span>
              <span className="stat-label">prosek</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{getBestScore()}%</span>
              <span className="stat-label">najbolji</span>
            </div>
            <div className="stat-item">
              <span className="stat-value">{results.length}</span>
              <span className="stat-label">ukupno</span>
            </div>
          </div>
        )}
      </div>

      {results.length > 0 && (
        <div className="controls-bar">
          <input
            type="text"
            placeholder="Pretraži kvizove..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
          <select 
            value={sortBy} 
            onChange={(e) => setSortBy(e.target.value)}
            className="sort-select"
          >
            <option value="date">Najnoviji</option>
            <option value="score">Najbolji rezultat</option>
            <option value="time">Najbrži</option>
          </select>
        </div>
      )}

      <div className="results-section">
        {processedResults.length === 0 ? (
          <div className="empty-state">
            {searchTerm ? (
              <>
                <p>Nema rezultata za "{searchTerm}"</p>
                <button onClick={() => setSearchTerm('')} className="clear-search">Obriši pretragu</button>
              </>
            ) : (
              <p>Još uvek nemate rezultate kvizova</p>
            )}
          </div>
        ) : (
          <MyResultsList results={processedResults} />
        )}
      </div>

      {pagination && pagination.totalPages > 1 && (
        <div className="pagination-section">
          <Pagination pagination={pagination} onPageChange={setPage} />
        </div>
      )}
    </div>
  );
}
