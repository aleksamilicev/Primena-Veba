import React, { useEffect, useState } from "react";
import { useParams, Link } from "react-router-dom";
import axios from "axios";
import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Legend,
} from "recharts";

const DetailedResults = () => {
  const { resultId } = useParams();
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(true);

  const token = localStorage.getItem("token");

  useEffect(() => {
    const fetchResult = async () => {
      try {
        const res = await axios.get(
          `https://localhost:7038/api/quizresults/${resultId}/details`,
          {
            headers: { Authorization: `Bearer ${token}` },
          }
        );
        setResult(res.data);
      } catch (error) {
        console.error("Failed to fetch result:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchResult();
  }, [resultId, token]);

  if (loading) return <p>Loading detailed results...</p>;
  if (!result) return <p>No result found.</p>;

  // PieChart data
  const pieData = [
    { name: "Correct", value: result.correctAnswers },
    { name: "Wrong", value: result.totalQuestions - result.correctAnswers },
  ];

  const COLORS = ["#4CAF50", "#F44336"];

  // LineChart data (progress over time)
  const lineData =
    result.attemptsHistory?.map((a) => ({
      date: new Date(a.completedAt).toLocaleDateString(),
      score: a.scorePercentage,
    })) || [];

  return (
    <div className="p-6">
      <h2 className="text-2xl font-bold mb-4">
        Detailed Results for {result.quizTitle}
      </h2>

      <p className="mb-2">
        Score: {result.correctAnswers} / {result.totalQuestions}
      </p>
      <p className="mb-2">
        Date: {new Date(result.completedAt).toLocaleString()}
      </p>
      <p className="mb-6">⏱ Time Taken: {result.timeTaken} seconds</p>

      {/* Grafikon napretka */}
      {lineData.length > 1 && (
        <div className="bg-white shadow rounded-xl p-4 mb-6">
          <h3 className="text-lg font-semibold mb-2">Progress Over Time</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={lineData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis domain={[0, 100]} />
              <Tooltip />
              <Legend />
              <Line
                type="monotone"
                dataKey="score"
                stroke="#2563eb"
                strokeWidth={2}
              />
            </LineChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Pie Chart */}
      <div className="bg-white shadow rounded-xl p-4 mb-6">
        <h3 className="text-lg font-semibold mb-2">Overall Performance</h3>
        <ResponsiveContainer width="100%" height={250}>
          <PieChart>
            <Pie
              data={pieData}
              dataKey="value"
              nameKey="name"
              cx="50%"
              cy="50%"
              outerRadius={80}
              label
            >
              {pieData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={COLORS[index]} />
              ))}
            </Pie>
            <Tooltip />
          </PieChart>
        </ResponsiveContainer>
      </div>

      {/* Lista pitanja */}
      <div className="bg-white shadow rounded-xl p-4">
        <h3 className="text-lg font-semibold mb-4">Question Breakdown</h3>
        <ul className="space-y-3">
          {result.answers.map((a, idx) => (
            <li
              key={a.answerId}
              className={`p-3 rounded-lg ${
                a.isCorrect ? "bg-green-100" : "bg-red-100"
              }`}
            >
              <p className="font-medium">
                Q{idx + 1}: {a.questionText}
              </p>
              <p>✅ Correct Answer: {a.correctAnswer}</p>
              <p>📝 Your Answer: {a.userAnswer}</p>
            </li>
          ))}
        </ul>
      </div>

      <div className="mt-6">
        <Link
          to="/results"
          className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
        >
          Back to Results
        </Link>
      </div>
    </div>
  );
};

export default DetailedResults;
