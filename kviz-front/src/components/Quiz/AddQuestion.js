import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { addQuestionToQuiz } from "../../api/services/quizService";

const AddQuestion = () => {
  const { quizId } = useParams();
  const navigate = useNavigate();
  
  // Osnovna polja
  const [questionText, setQuestionText] = useState("");
  const [questionType, setQuestionType] = useState("true-false");
  const [difficultyLevel, setDifficultyLevel] = useState("Easy");
  const [correctAnswer, setCorrectAnswer] = useState("");
  
  // Polja za opcije (multi-select i one-select)
  const [options, setOptions] = useState([
    { letter: "A", text: "" },
    { letter: "B", text: "" },
    { letter: "C", text: "" },
    { letter: "D", text: "" }
  ]);
  
  // Za multi-select
  const [selectedMultipleAnswers, setSelectedMultipleAnswers] = useState([]);
  
  const [message, setMessage] = useState("");
  const [loading, setLoading] = useState(false);

  // Tipovi pitanja sa opisima
  const questionTypes = [
    {
      value: "true-false",
      label: "True/False",
      description: "Pitanje sa tačno/netačno odgovorom"
    },
    {
      value: "fill-in-the-blank",
      label: "Fill in the Blank",
      description: "Pitanje gde korisnik unosi proizvoljan tekst"
    },
    {
      value: "one-select",
      label: "Multiple Choice (One Answer)",
      description: "Pitanje sa jednim tačnim odgovorom od ponuđenih"
    },
    {
      value: "multi-select",
      label: "Multiple Choice (Multiple Answers)",
      description: "Pitanje sa više tačnih odgovora"
    }
  ];

  const difficultyLevels = ["Easy", "Medium", "Hard"];

  // Reset polja kada se menja tip pitanja
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
    switch (questionType) {
      case "true-false":
        return correctAnswer;
      case "fill-in-the-blank":
        return correctAnswer;
      case "one-select":
        return correctAnswer;
      case "multi-select":
        return selectedMultipleAnswers.join(",");
      default:
        return correctAnswer;
    }
  };

  const validateForm = () => {
    if (!questionText.trim()) {
      setMessage("Molimo unesite tekst pitanja");
      return false;
    }

    switch (questionType) {
      case "true-false":
        if (correctAnswer !== "True" && correctAnswer !== "False") {
          setMessage("Za True/False pitanje, molimo izaberite True ili False");
          return false;
        }
        break;
      case "fill-in-the-blank":
        if (!correctAnswer.trim()) {
          setMessage("Molimo unesite tačan odgovor");
          return false;
        }
        break;
      case "one-select":
        const hasOptions = options.some(opt => opt.text.trim() !== "");
        if (!hasOptions) {
          setMessage("Molimo unesite bar jednu opciju");
          return false;
        }
        if (!correctAnswer) {
          setMessage("Molimo izaberite tačan odgovor");
          return false;
        }
        break;
      case "multi-select":
        const hasMultiOptions = options.some(opt => opt.text.trim() !== "");
        if (!hasMultiOptions) {
          setMessage("Molimo unesite bar jednu opciju");
          return false;
        }
        if (selectedMultipleAnswers.length === 0) {
          setMessage("Molimo izaberite bar jedan tačan odgovor");
          return false;
        }
        break;
    }
    return true;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    setMessage("");

    try {
      const questionData = {
        quizId: parseInt(quizId),
        questionText: formatQuestionTextWithOptions(),
        questionType: questionType,
        difficultyLevel: difficultyLevel,
        correctAnswer: getCorrectAnswerValue()
      };

      console.log("Sending question data:", questionData);

      await addQuestionToQuiz(quizId, questionData);
      alert("Pitanje je uspešno dodato!");
      navigate("/quizzes");
    } catch (err) {
      console.error("Error adding question:", err);
      setMessage(err.response?.data?.message || "Greška pri dodavanju pitanja");
    } finally {
      setLoading(false);
    }
  };

  const renderQuestionTypeSpecificFields = () => {
    switch (questionType) {
      case "true-false":
        return (
          <div style={{ marginBottom: "15px" }}>
            <label>Tačan odgovor:</label>
            <select 
              value={correctAnswer} 
              onChange={(e) => setCorrectAnswer(e.target.value)}
              required
              style={{ width: "100%", padding: "8px", marginTop: "5px" }}
            >
              <option value="">Izaberite...</option>
              <option value="True">True</option>
              <option value="False">False</option>
            </select>
          </div>
        );

      case "fill-in-the-blank":
        return (
          <div style={{ marginBottom: "15px" }}>
            <label>Tačan odgovor:</label>
            <input
              type="text"
              placeholder="Unesite tačan odgovor"
              value={correctAnswer}
              onChange={(e) => setCorrectAnswer(e.target.value)}
              required
              style={{ width: "100%", padding: "8px", marginTop: "5px" }}
            />
          </div>
        );

      case "one-select":
        return (
          <div>
            <div style={{ marginBottom: "15px" }}>
              <label>Opcije:</label>
              {options.map((option, index) => (
                <div key={option.letter} style={{ marginBottom: "8px" }}>
                  <input
                    type="text"
                    placeholder={`Opcija ${option.letter}`}
                    value={option.text}
                    onChange={(e) => handleOptionChange(index, e.target.value)}
                    style={{ width: "100%", padding: "8px" }}
                  />
                </div>
              ))}
            </div>
            <div style={{ marginBottom: "15px" }}>
              <label>Tačan odgovor:</label>
              <select 
                value={correctAnswer} 
                onChange={(e) => setCorrectAnswer(e.target.value)}
                required
                style={{ width: "100%", padding: "8px", marginTop: "5px" }}
              >
                <option value="">Izaberite tačan odgovor...</option>
                {options
                  .filter(opt => opt.text.trim() !== "")
                  .map(opt => (
                    <option key={opt.letter} value={opt.letter}>
                      {opt.letter}: {opt.text}
                    </option>
                  ))
                }
              </select>
            </div>
          </div>
        );

      case "multi-select":
        return (
          <div>
            <div style={{ marginBottom: "15px" }}>
              <label>Opcije:</label>
              {options.map((option, index) => (
                <div key={option.letter} style={{ marginBottom: "8px" }}>
                  <input
                    type="text"
                    placeholder={`Opcija ${option.letter}`}
                    value={option.text}
                    onChange={(e) => handleOptionChange(index, e.target.value)}
                    style={{ width: "100%", padding: "8px" }}
                  />
                </div>
              ))}
            </div>
            <div style={{ marginBottom: "15px" }}>
              <label>Tačni odgovori (izaberite sve tačne):</label>
              <div style={{ marginTop: "8px" }}>
                {options
                  .filter(opt => opt.text.trim() !== "")
                  .map(opt => (
                    <div key={opt.letter} style={{ marginBottom: "5px" }}>
                      <label style={{ display: "flex", alignItems: "center" }}>
                        <input
                          type="checkbox"
                          checked={selectedMultipleAnswers.includes(opt.letter)}
                          onChange={() => handleMultiSelectChange(opt.letter)}
                          style={{ marginRight: "8px" }}
                        />
                        {opt.letter}: {opt.text}
                      </label>
                    </div>
                  ))
                }
              </div>
              {selectedMultipleAnswers.length > 0 && (
                <p style={{ marginTop: "8px", color: "#666", fontSize: "14px" }}>
                  Izabrani tačni odgovori: {selectedMultipleAnswers.join(", ")}
                </p>
              )}
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div style={{ maxWidth: "600px", margin: "0 auto", padding: "20px" }}>
      <h2>Dodaj pitanje u kviz {quizId}</h2>
      
      <form onSubmit={handleSubmit}>
        {/* Tip pitanja */}
        <div style={{ marginBottom: "15px" }}>
          <label>Tip pitanja:</label>
          <select 
            value={questionType} 
            onChange={(e) => setQuestionType(e.target.value)}
            style={{ width: "100%", padding: "8px", marginTop: "5px" }}
          >
            {questionTypes.map(type => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
          <p style={{ fontSize: "12px", color: "#666", marginTop: "5px" }}>
            {questionTypes.find(t => t.value === questionType)?.description}
          </p>
        </div>

        {/* Nivo težine */}
        <div style={{ marginBottom: "15px" }}>
          <label>Nivo težine:</label>
          <select 
            value={difficultyLevel} 
            onChange={(e) => setDifficultyLevel(e.target.value)}
            style={{ width: "100%", padding: "8px", marginTop: "5px" }}
          >
            {difficultyLevels.map(level => (
              <option key={level} value={level}>{level}</option>
            ))}
          </select>
        </div>

        {/* Tekst pitanja */}
        <div style={{ marginBottom: "15px" }}>
          <label>Tekst pitanja:</label>
          <textarea
            placeholder="Unesite tekst pitanja"
            value={questionText}
            onChange={(e) => setQuestionText(e.target.value)}
            required
            rows="3"
            style={{ width: "100%", padding: "8px", marginTop: "5px", resize: "vertical" }}
          />
        </div>

        {/* Specifična polja za tip pitanja */}
        {renderQuestionTypeSpecificFields()}

        <button 
          type="submit" 
          disabled={loading}
          style={{ 
            width: "100%", 
            padding: "12px", 
            backgroundColor: loading ? "#ccc" : "#007bff", 
            color: "white", 
            border: "none", 
            borderRadius: "4px", 
            cursor: loading ? "not-allowed" : "pointer" 
          }}
        >
          {loading ? "Dodavanje..." : "Dodaj pitanje"}
        </button>
      </form>

      {message && (
        <p style={{ 
          marginTop: "15px", 
          padding: "10px", 
          backgroundColor: message.includes("Greška") || message.includes("Molimo") ? "#f8d7da" : "#d4edda", 
          color: message.includes("Greška") || message.includes("Molimo") ? "#721c24" : "#155724",
          border: "1px solid " + (message.includes("Greška") || message.includes("Molimo") ? "#f5c6cb" : "#c3e6cb"),
          borderRadius: "4px"
        }}>
          {message}
        </p>
      )}
    </div>
  );
};

export default AddQuestion;