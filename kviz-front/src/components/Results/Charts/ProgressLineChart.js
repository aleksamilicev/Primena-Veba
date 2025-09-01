import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";

export default function ProgressLineChart({ history }) {
  if (!history || history.length < 2) return null;

  const data = history.map((a) => ({
    date: new Date(a.completedAt).toLocaleDateString(),
    score: a.scorePercentage,
  }));

  return (
    <ResponsiveContainer width="100%" height={300}>
      <LineChart data={data}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="date" />
        <YAxis domain={[0, 100]} />
        <Tooltip />
        <Legend />
        <Line type="monotone" dataKey="score" stroke="#2563eb" strokeWidth={2} />
      </LineChart>
    </ResponsiveContainer>
  );
}
