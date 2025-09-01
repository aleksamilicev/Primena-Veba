import { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";
import { fetchAllResults } from "../../api/services/resultService";
import { Result } from "../../api/models/Result";
import ResultsTable from "../../components/Results/ResultsTable";
import Pagination from "../../components/Results/Pagination";

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
    return <p className="text-red-500 text-center mt-6">Access denied</p>;
  }

  if (loading) return <p className="text-center mt-6">Loading results...</p>;
  if (error) return <p className="text-center text-red-500 mt-6">{error}</p>;

  return (
    <div className="max-w-6xl mx-auto mt-10 p-6 bg-white shadow-lg rounded-2xl">
      <h2 className="text-2xl font-bold mb-4 text-center">📊 All Users' Results</h2>
      {results.length === 0 ? (
        <p className="text-gray-500 text-center">No results found.</p>
      ) : (
        <>
          <ResultsTable results={results} />
          <Pagination pagination={pagination} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}
