export default function ResultsTable({ results }) {
  return (
    <table className="w-full border-collapse border border-gray-300">
      <thead>
        <tr className="bg-gray-100">
          <th className="border border-gray-300 px-4 py-2">User</th>
          <th className="border border-gray-300 px-4 py-2">Email</th>
          <th className="border border-gray-300 px-4 py-2">Quiz</th>
          <th className="border border-gray-300 px-4 py-2">Score</th>
          <th className="border border-gray-300 px-4 py-2">Time Taken</th>
          <th className="border border-gray-300 px-4 py-2">Date</th>
        </tr>
      </thead>
      <tbody>
        {results.map((result) => (
          <tr key={result.resultId} className="hover:bg-gray-50">
            <td className="border border-gray-300 px-4 py-2">{result.username}</td>
            <td className="border border-gray-300 px-4 py-2">{result.userEmail}</td>
            <td className="border border-gray-300 px-4 py-2">{result.quizTitle}</td>
            <td className="border border-gray-300 px-4 py-2">
              {result.correctAnswers}/{result.totalQuestions} ({result.scorePercentage}%)
            </td>
            <td className="border border-gray-300 px-4 py-2">{result.timeTaken}s</td>
            <td className="border border-gray-300 px-4 py-2">
              {new Date(result.completedAt).toLocaleString()}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
