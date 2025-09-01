import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer } from "recharts";

export default function PerformancePieChart({ correct, wrong }) {
  const data = [
    { name: "Correct", value: correct },
    { name: "Wrong", value: wrong },
  ];
  const COLORS = ["#4CAF50", "#F44336"];

  return (
    <ResponsiveContainer width="100%" height={250}>
      <PieChart>
        <Pie data={data} dataKey="value" nameKey="name" outerRadius={80} label>
          {data.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={COLORS[index]} />
          ))}
        </Pie>
        <Tooltip />
      </PieChart>
    </ResponsiveContainer>
  );
}
