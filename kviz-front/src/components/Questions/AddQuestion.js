// src/components/Questions/AddQuestion.js
import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { addQuestionToQuiz } from "../../api/services/quizService";

const AddQuestion = () => {
  const { quizId } = useParams();
  const navigate = useNavigate();
  
  const [questionText, setQuestionText] = useState("");
  const [questionType, setQuestionType] = useState("true-false");
  const [difficultyLevel, setDifficultyLevel] = useState("Easy");
  const [correctAnswer, setCorrectAnswer] = useState("");
  const [options, setOptions] = useState([
    { letter: "A", text: "" },
    { letter: "B", text: "" },
    { letter: "C", text: "" },
    { letter: "D", text: "" }
  ]);
  const [selectedMultipleAnswers, setSelectedMultipleAnswers] = useState([]);
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  const questionTypes = [
    { value: "true-false", label: "True/False", description: "Pitanje sa tačno/netačno odgovorom" },
    { value: "fill-in-the-blank", label: "Fill in the Blank", description: "Pitanje gde korisnik unosi proizvoljan tekst" },
    { value: "one-select", label: "Multiple Choice (One Answer)", description: "Pitanje sa jednim tačnim odgovorom od ponuđenih" },
    { value: "multi-select", label: "Multiple Choice (Multiple Answers)", description: "Pitanje sa više tačnih odgovora" }
  ];

  const difficultyLevels = ["Easy", "Medium", "Hard"];

  useEffect(() => {
    setCorrectAnswer("");
    setSelectedMultipleAnswers([]);
    setOptions([
      { letter: "A", text: "" },
      { letter: "B", text: "" },
      { letter: "C", text: "" },
      { letter: "D", text: "" }
    ]);
  }, [questionType]);

  const handleOptionChange = (index, value) => {
    const newOptions = [...options];
    newOptions[index].text = value;
    setOptions(newOptions);
  };

  const handleMultiSelectChange = (letter) => {
    if (selectedMultipleAnswers.includes(letter)) {
      setSelectedMultipleAnswers(selectedMultipleAnswers.filter(ans => ans !== letter));
    } else {
      setSelectedMultipleAnswers([...selectedMultipleAnswers, letter]);
    }
  };

  const formatQuestionTextWithOptions = () => {
    if (questionType === "one-select" || questionType === "multi-select") {
      const optionsText = options
        .filter(opt => opt.text.trim() !== "")
        .map(opt => `${opt.letter}:${opt.text}`)
        .join(", ");
      return `${questionText} ${optionsText}`;
    }
    return questionText;
  };

  const getCorrectAnswerValue = () => {
    if (questionType === "multi-select") return selectedMultipleAnswers.join(",");
    return correctAnswer;
  };

  const validateForm = () => {
    if (!questionText.trim()) { setMessage("Molimo unesite tekst pitanja"); return false; }
    if (questionType === "true-false" && !["True","False"].includes(correctAnswer)) {
      setMessage("Za True/False pitanje, molimo izaberite True ili False"); return false;
    }
    if ((questionType === "one-select" || questionType === "multi-select") && options.every(opt => !opt.text.trim())) {
      setMessage("Molimo unesite bar jednu opciju"); return false;
    }
    if ((questionType === "one-select" && !correctAnswer) || (questionType === "multi-select" && selectedMultipleAnswers.length === 0)) {
      setMessage("Molimo izaberite tačan odgovor"); return false;
    }
    if (questionType === "fill-in-the-blank" && !correctAnswer.trim()) {
      setMessage("Molimo unesite tačan odgovor"); return false;
    }
    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!validateForm()) return;

    setLoading(true);
    setMessage("");

    try {
      const questionData = {
        questionText: formatQuestionTextWithOptions(),
        questionType,
        difficultyLevel,
        correctAnswer: getCorrectAnswerValue()
      };

      await addQuestionToQuiz(quizId, questionData);
      alert("Pitanje je uspešno dodato!");
      navigate("/quizzes");
    } catch (err) {
      console.error(err);
      setMessage(err.response?.data?.message || "Greška pri dodavanju pitanja");
    } finally {
      setLoading(false);
    }
  };

  // renderQuestionTypeSpecificFields ostaje isto...

  return (
    <div style={{ maxWidth: "600px", margin: "0 auto", padding: "20px" }}>
      <h2>Dodaj pitanje u kviz {quizId}</h2>
      <form onSubmit={handleSubmit}>
        {/* Ostatak forme ostaje isto */}
      </form>
      {message && <p>{message}</p>}
    </div>
  );
};

export default AddQuestion;
