import { useEffect, useState } from "react";
import { useAuth } from "../../context/AuthContext";

export default function Ranking() {
  const { user } = useAuth();
  const [rankings, setRankings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [period, setPeriod] = useState("all");
  const [quizId, setQuizId] = useState(""); // za filter po kvizu

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

  const fetchRankings = async () => {
    setLoading(true);
    setError(null);
    try {
      let url = `/ranking?period=${period}`;
      if (quizId.trim() !== "") {
        url = `/ranking/${quizId}?period=${period}`;
      }
      const data = await apiCall(url);
      setRankings(data);
    } catch (err) {
      console.error(err);
      setError("Failed to load rankings");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRankings();
  }, [period, quizId]);

  return (
    <div className="max-w-5xl mx-auto mt-10 p-6 bg-white shadow-lg rounded-2xl">
      <h2 className="text-3xl font-bold mb-6 text-center">🏆 Global Ranking</h2>

      {/* Filter controls */}
      <div className="flex flex-col md:flex-row justify-between items-center gap-4 mb-6">
        {/* Period buttons */}
        <div className="flex gap-3">
          {["week", "month", "all"].map((p) => (
            <button
              key={p}
              className={`px-4 py-2 rounded-lg font-medium transition-colors ${
                period === p ? "bg-blue-500 text-white shadow-md" : "bg-gray-200 hover:bg-gray-300"
              }`}
              onClick={() => setPeriod(p)}
            >
              {p === "week" ? "This Week" : p === "month" ? "This Month" : "All Time"}
            </button>
          ))}
        </div>

        {/* Quiz ID filter */}
        <div className="flex gap-2 items-center">
          <label className="font-medium">Filter by Quiz ID:</label>
          <input
            type="number"
            min="1"
            placeholder="Quiz ID"
            value={quizId}
            onChange={(e) => setQuizId(e.target.value)}
            className="border border-gray-300 rounded px-3 py-1 w-24 focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
          <button
            onClick={fetchRankings}
            className="bg-green-500 text-white px-3 py-1 rounded hover:bg-green-600 transition"
          >
            Apply
          </button>
        </div>
      </div>

      {loading && <p className="text-center text-gray-600">Loading rankings...</p>}
      {error && <p className="text-center text-red-500">{error}</p>}
      {!loading && !error && rankings.length === 0 && (
        <p className="text-gray-500 text-center">No rankings available.</p>
      )}

      {!loading && rankings.length > 0 && (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse border border-gray-300">
            <thead>
              <tr className="bg-gray-100">
                <th className="border border-gray-300 px-4 py-2">Rank</th>
                <th className="border border-gray-300 px-4 py-2">User</th>
                <th className="border border-gray-300 px-4 py-2">Quiz</th>
                <th className="border border-gray-300 px-4 py-2">Score</th>
                <th className="border border-gray-300 px-4 py-2">Time Taken</th>
                <th className="border border-gray-300 px-4 py-2">Completed</th>
              </tr>
            </thead>
            <tbody>
              {rankings.map((r, idx) => (
                <tr
                  key={`${r.Quiz_Id}-${r.User_Id}-${idx}`}
                  className={`hover:bg-gray-50 transition ${idx % 2 === 0 ? "bg-gray-50" : "bg-white"}`}
                >
                  <td className="border border-gray-300 px-4 py-2 font-semibold">{r.Rank_Position}</td>
                  <td className="border border-gray-300 px-4 py-2">{r.Username}</td>
                  <td className="border border-gray-300 px-4 py-2">{r.Quiz_Id ?? quizId}</td>
                  <td className="border border-gray-300 px-4 py-2">{r.Score_Percentage}%</td>
                  <td className="border border-gray-300 px-4 py-2">{r.Time_Taken}s</td>
                  <td className="border border-gray-300 px-4 py-2">
                    {new Date(r.Completed_At).toLocaleString()}
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
