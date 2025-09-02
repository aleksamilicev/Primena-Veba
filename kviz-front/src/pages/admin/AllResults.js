import { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import { Link } from 'react-router-dom';
import { fetchAllResults } from "../../api/services/resultService";
import { Result } from "../../api/models/Result";
import ResultsTable from "../../components/Results/ResultsTable";
import Pagination from "../../components/Results/Pagination";
import "../../styles/AllResults.css";

export default function AllResults() {
  const { user } = useAuth();

  const [results, setResults] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const [page, setPage] = useState(1);
  const pageSize = 20;

  const loadResults = async (page) => {
    try {
      setLoading(true);
      const data = await fetchAllResults(page, pageSize);
      setResults(data.results.map((r) => new Result(r)));
      setPagination(data.pagination);
    } catch (err) {
      console.error("Greška pri dohvatanju rezultata:", err);
      setError("Failed to load results");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (user?.isAdmin) {
      loadResults(page);
    }
  }, [user, page]);

  if (!user?.isAdmin) {
    return <p className="allresults-error">Access denied</p>;
  }

  if (loading) return <p className="allresults-loading">Loading results...</p>;
  if (error) return <p className="allresults-error">{error}</p>;

  return (
    <div className="allresults-container">
                    <Link to="/quizzes" className="nav-link">
                📚 Quizzes
              </Link>
      <h2 className="allresults-title">📊 All Users' Results</h2>

      {results.length === 0 ? (
        <p className="allresults-empty">No results found.</p>
      ) : (
        <>
          <ResultsTable results={results} />
          <Pagination pagination={pagination} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}
