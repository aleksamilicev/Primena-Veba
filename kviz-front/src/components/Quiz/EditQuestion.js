import React, { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { updateQuestionById, getQuestionById } from "../../api/services/quizService";

const EditQuestion = () => {
  const { questionId } = useParams();
  const navigate = useNavigate();
  
  const [questionData, setQuestionData] = useState({
    questionText: "",
    questionType: "",
    correctAnswer: "",
    difficultyLevel: ""
  });
  
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const questionTypes = [
    { value: "true-false", label: "True/False" },
    { value: "fill-in-the-blank", label: "Fill in the Blank" },
    { value: "one-select", label: "Multiple Choice (One Answer)" },
    { value: "multi-select", label: "Multiple Choice (Multiple Answers)" }
  ];

  const difficultyLevels = ["Easy", "Medium", "Hard"];

  useEffect(() => {
    const fetchQuestion = async () => {
      try {
        setLoading(true);
        const response = await getQuestionById(questionId);
        console.log("Fetched question:", response); // za debug
        
        // Mapiranje backend naziva na frontend nazive
        setQuestionData({
          questionText: response.Question_Text || response.questionText || "",
          questionType: response.Question_Type || response.questionType || "",
          correctAnswer: response.Correct_Answer || response.correctAnswer || "",
          difficultyLevel: response.Difficulty_Level || response.difficultyLevel || ""
        });
      } catch (error) {
        console.error("Error fetching question:", error);
        alert("Greška pri učitavanju pitanja.");
        navigate(-1);
      } finally {
        setLoading(false);
      }
    };

    fetchQuestion();
  }, [questionId, navigate]);

  const handleChange = (e) => {
    setQuestionData({ 
      ...questionData, 
      [e.target.name]: e.target.value 
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!questionData.questionText.trim()) {
      alert("Molimo unesite tekst pitanja");
      return;
    }

    setSaving(true);
    
    try {
      await updateQuestionById(questionId, questionData);
      alert("Pitanje je uspešno ažurirano!");
      navigate(-1); // vrati na prethodnu stranicu
    } catch (error) {
      console.error("Error updating question:", error);
      alert("Greška pri ažuriranju pitanja.");
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <div>Učitavanje pitanja...</div>;

  return (
    <div style={{ maxWidth: "600px", margin: "0 auto", padding: "20px" }}>
      <h2>Uredi pitanje</h2>
      
      <form onSubmit={handleSubmit}>
        {/* Tekst pitanja */}
        <div style={{ marginBottom: "15px" }}>
          <label style={{ display: "block", marginBottom: "5px", fontWeight: "bold" }}>
            Tekst pitanja:
          </label>
          <textarea
            name="questionText"
            placeholder="Unesite tekst pitanja"
            value={questionData.questionText}
            onChange={handleChange}
            required
            rows="4"
            style={{ 
              width: "100%", 
              padding: "8px", 
              border: "1px solid #ddd", 
              borderRadius: "4px",
              resize: "vertical"
            }}
          />
        </div>

        {/* Tip pitanja */}
        <div style={{ marginBottom: "15px" }}>
          <label style={{ display: "block", marginBottom: "5px", fontWeight: "bold" }}>
            Tip pitanja:
          </label>
          <select
            name="questionType"
            value={questionData.questionType}
            onChange={handleChange}
            required
            style={{ 
              width: "100%", 
              padding: "8px", 
              border: "1px solid #ddd", 
              borderRadius: "4px"
            }}
          >
            <option value="">Izaberite tip pitanja</option>
            {questionTypes.map(type => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
        </div>

        {/* Nivo težine */}
        <div style={{ marginBottom: "15px" }}>
          <label style={{ display: "block", marginBottom: "5px", fontWeight: "bold" }}>
            Nivo težine:
          </label>
          <select
            name="difficultyLevel"
            value={questionData.difficultyLevel}
            onChange={handleChange}
            required
            style={{ 
              width: "100%", 
              padding: "8px", 
              border: "1px solid #ddd", 
              borderRadius: "4px"
            }}
          >
            <option value="">Izaberite nivo težine</option>
            {difficultyLevels.map(level => (
              <option key={level} value={level}>
                {level}
              </option>
            ))}
          </select>
        </div>

        {/* Tačan odgovor */}
        <div style={{ marginBottom: "15px" }}>
          <label style={{ display: "block", marginBottom: "5px", fontWeight: "bold" }}>
            Tačan odgovor:
          </label>
          <textarea
            name="correctAnswer"
            placeholder="Unesite tačan odgovor"
            value={questionData.correctAnswer}
            onChange={handleChange}
            required
            rows="2"
            style={{ 
              width: "100%", 
              padding: "8px", 
              border: "1px solid #ddd", 
              borderRadius: "4px",
              resize: "vertical"
            }}
          />
          <small style={{ color: "#666", fontSize: "12px" }}>
            Format zavisi od tipa pitanja (True/False, tekst, A,B,C ili A)
          </small>
        </div>

        {/* Dugmad */}
        <div style={{ display: "flex", gap: "10px" }}>
          <button 
            type="submit" 
            disabled={saving}
            style={{ 
              flex: 1,
              padding: "12px", 
              backgroundColor: saving ? "#6c757d" : "#007bff", 
              color: "white", 
              border: "none", 
              borderRadius: "4px", 
              cursor: saving ? "not-allowed" : "pointer",
              fontWeight: "bold"
            }}
          >
            {saving ? "Čuvanje..." : "Sačuvaj izmene"}
          </button>
          
          <button 
            type="button" 
            onClick={() => navigate(-1)}
            style={{ 
              flex: 1,
              padding: "12px", 
              backgroundColor: "#6c757d", 
              color: "white", 
              border: "none", 
              borderRadius: "4px", 
              cursor: "pointer",
              fontWeight: "bold"
            }}
          >
            Otkaži
          </button>
        </div>
      </form>
    </div>
  );
};

export default EditQuestion;