import "../../styles/ResultsTable.css";

export default function ResultsTable({ results }) {
  return (
    <table className="results-table">
      <thead>
        <tr>
          <th>User</th>
          <th>Email</th>
          <th>Quiz</th>
          <th>Score</th>
          <th>Time Taken</th>
          <th>Date</th>
        </tr>
      </thead>
      <tbody>
        {results.map((result) => (
          <tr key={result.resultId}>
            <td>{result.username}</td>
            <td>{result.userEmail}</td>
            <td>{result.quizTitle}</td>
            <td>
              {result.correctAnswers}/{result.totalQuestions} (
              {result.scorePercentage}%)
            </td>
            <td>{result.timeTaken}s</td>
            <td>{new Date(result.completedAt).toLocaleString()}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
