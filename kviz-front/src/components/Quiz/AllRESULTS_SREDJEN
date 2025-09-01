import { useEffect, useState } from "react";
import axios from "axios";
import { useAuth } from "../../context/AuthContext";
import "../../styles/AllResults.css"; // dodajte ovaj CSS fajl

export default function AllResults() {
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [sortField, setSortField] = useState('completedAt');
  const [sortDirection, setSortDirection] = useState('desc');
  const [searchTerm, setSearchTerm] = useState('');
  const { user } = useAuth();

  useEffect(() => {
    if (!user?.isAdmin) return;

    const fetchResults = async () => {
      try {
        setLoading(true);
        const token = localStorage.getItem("token");
        const res = await axios.get(
          "https://localhost:7038/api/quizresults/admin/all-results?page=1&pageSize=20",
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
        setResults(res.data.results || []);
      } catch (err) {
        console.error("Greška pri dohvatanju rezultata:", err);
        setError("Neuspešno učitavanje rezultata. Molimo pokušajte ponovo.");
      } finally {
        setLoading(false);
      }
    };

    fetchResults();
  }, [user]);

  // Sortiranje rezultata
  const sortedResults = [...results].sort((a, b) => {
    let aValue = a[sortField];
    let bValue = b[sortField];

    if (sortField === 'completedAt') {
      aValue = new Date(aValue);
      bValue = new Date(bValue);
    }

    if (sortField === 'scorePercentage') {
      aValue = parseFloat(aValue);
      bValue = parseFloat(bValue);
    }

    if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
    if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
    return 0;
  });

  // Filtriranje rezultata
  const filteredResults = sortedResults.filter(result =>
    result.username?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    result.userEmail?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    result.quizTitle?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const handleSort = (field) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('asc');
    }
  };

  const getScoreColor = (percentage) => {
    if (percentage >= 80) return 'excellent';
    if (percentage >= 60) return 'good';
    if (percentage >= 40) return 'average';
    return 'poor';
  };

  if (!user?.isAdmin) {
    return (
      <div className="access-denied">
        <div className="access-denied-content">
          <h2>🚫 Pristup odbijen</h2>
          <p>Nemate dozvolu za pristup ovoj stranici.</p>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Učitavanje rezultata...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="error-container">
        <div className="error-content">
          <h3>⚠️ Greška</h3>
          <p>{error}</p>
          <button 
            onClick={() => window.location.reload()} 
            className="retry-button"
          >
            Pokušaj ponovo
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="results-container">
      <div className="results-header">
        <h2 className="results-title">
          📊 Rezultati svih korisnika
        </h2>
        <div className="results-stats">
          <span className="stat-badge">
            Ukupno: {results.length} rezultata
          </span>
        </div>
      </div>

      <div className="controls-section">
        <div className="search-container">
          <input
            type="text"
            placeholder="🔍 Pretraži po korisniku, email-u ili kvizu..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
        </div>
      </div>

      {filteredResults.length === 0 ? (
        <div className="no-results">
          <h3>📋 Nema rezultata</h3>
          <p>
            {searchTerm 
              ? "Nema rezultata koji odgovaraju vašoj pretrazi." 
              : "Još uvek nema rezultata kvizova."
            }
          </p>
        </div>
      ) : (
        <div className="table-container">
          <table className="results-table">
            <thead>
              <tr>
                <th onClick={() => handleSort('username')} className="sortable">
                  👤 Korisnik
                  {sortField === 'username' && (
                    <span className="sort-indicator">
                      {sortDirection === 'asc' ? ' ↑' : ' ↓'}
                    </span>
                  )}
                </th>
                <th onClick={() => handleSort('userEmail')} className="sortable">
                  📧 Email
                  {sortField === 'userEmail' && (
                    <span className="sort-indicator">
                      {sortDirection === 'asc' ? ' ↑' : ' ↓'}
                    </span>
                  )}
                </th>
                <th onClick={() => handleSort('quizTitle')} className="sortable">
                  📝 Kviz
                  {sortField === 'quizTitle' && (
                    <span className="sort-indicator">
                      {sortDirection === 'asc' ? ' ↑' : ' ↓'}
                    </span>
                  )}
                </th>
                <th onClick={() => handleSort('scorePercentage')} className="sortable">
                  🎯 Rezultat
                  {sortField === 'scorePercentage' && (
                    <span className="sort-indicator">
                      {sortDirection === 'asc' ? ' ↑' : ' ↓'}
                    </span>
                  )}
                </th>
                <th onClick={() => handleSort('timeTaken')} className="sortable">
                  ⏱️ Vreme
                  {sortField === 'timeTaken' && (
                    <span className="sort-indicator">
                      {sortDirection === 'asc' ? ' ↑' : ' ↓'}
                    </span>
                  )}
                </th>
                <th onClick={() => handleSort('completedAt')} className="sortable">
                  📅 Datum
                  {sortField === 'completedAt' && (
                    <span className="sort-indicator">
                      {sortDirection === 'asc' ? ' ↑' : ' ↓'}
                    </span>
                  )}
                </th>
              </tr>
            </thead>
            <tbody>
              {filteredResults.map((result) => (
                <tr key={result.resultId} className="table-row">
                  <td className="user-cell">
                    <div className="user-info">
                      <div className="user-avatar">
                        {result.username?.charAt(0).toUpperCase()}
                      </div>
                      <span className="username">{result.username}</span>
                    </div>
                  </td>
                  <td className="email-cell">{result.userEmail}</td>
                  <td className="quiz-cell">
                    <span className="quiz-title">{result.quizTitle}</span>
                  </td>
                  <td className="score-cell">
                    <div className="score-info">
                      <span className={`score-badge ${getScoreColor(result.scorePercentage)}`}>
                        {result.scorePercentage}%
                      </span>
                      <span className="score-details">
                        {result.correctAnswers}/{result.totalQuestions}
                      </span>
                    </div>
                  </td>
                  <td className="time-cell">
                    <span className="time-badge">
                      {result.timeTaken}s
                    </span>
                  </td>
                  <td className="date-cell">
                    {new Date(result.completedAt).toLocaleDateString('sr-RS', {
                      year: 'numeric',
                      month: 'short',
                      day: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}