export default function QuestionBreakdown({ answers }) {
  return (
    <ul className="space-y-3">
      {answers.map((a, idx) => (
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
  );
}
