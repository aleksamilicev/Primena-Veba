import React, { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { BookOpen, Clock, CheckCircle, XCircle, ArrowRight, ArrowLeft, Trophy } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import '../../styles/QuizTaking.css';

const QuizTaking = () => {
  const { quizId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  
  // State management
  const [currentQuiz, setCurrentQuiz] = useState(null);
  const [currentAttempt, setCurrentAttempt] = useState(null);
  const [userAnswers, setUserAnswers] = useState({});
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [quizResult, setQuizResult] = useState(null);
  const [loading, setLoading] = useState(false);
  const [view, setView] = useState('starting');
  const [error, setError] = useState(null);
  const location = useLocation();
  const [elapsedTime, setElapsedTime] = useState(0);

  const formatTime = (seconds) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${String(secs).padStart(2, "0")}`;
  };

  const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

  // API calls
  const apiCall = async (url, method = 'GET', body = null) => {
    const token = localStorage.getItem('token');
    
    const response = await fetch(`${API_BASE_URL}${url}`, {
      method,
      headers: {
        'Content-Type': 'application/json',
        'Authorization': token ? `Bearer ${token}` : '',
      },
      body: body ? JSON.stringify(body) : null
    });

    if (!response.ok) {
      const errorData = await response.text();
      throw new Error(errorData || `HTTP error! status: ${response.status}`);
    }

    return response.json();
  };

  useEffect(() => {
    let timer;
    if (view === "takingQuiz" && currentAttempt?.startedAt) {
      timer = setInterval(() => {
        const start = new Date(currentAttempt.startedAt);
        const now = new Date();
        const elapsed = Math.floor((now - start) / 1000);

        setElapsedTime(elapsed);

        // Auto-finish kad prođe 20 sekundi (stavi na 10minuta tj 600 sekundi)
        if (elapsed >= 600) {
          clearInterval(timer);
          finishQuiz();
        }
      }, 1000);
    }
    return () => clearInterval(timer);
  }, [view, currentAttempt]);

  // Koristimo useEffect da resetuje stanje kad se menja quizId
  useEffect(() => {
    console.log('Lokacija ili quizId se promenili:', location.pathname, quizId);
    
    setCurrentQuiz(null);
    setCurrentAttempt(null);
    setUserAnswers({});
    setCurrentQuestionIndex(0);
    setQuizResult(null);
    setElapsedTime(0);
    setView('starting');
    setError(null);
    setLoading(false);
    
    if (quizId) {
      console.log('Pokretanje kviza za ID:', quizId);
      startQuiz();
    }
  }, [quizId, location.pathname]);

  useEffect(() => {
    console.log('QuizTaking komponenta je mount-ovana');
    
    return () => {
      console.log('QuizTaking komponenta se unmount-uje - čišćenje stanja');
      resetQuizState();
    };
  }, []);

  const hasStartedQuiz = useRef(false);
  
  const startQuiz = async () => {
    if (hasStartedQuiz.current) return;
    hasStartedQuiz.current = true;
    setLoading(true);
    if (loading) {
      console.log('startQuiz već se izvršava, prekidamo dupli poziv');
      return;
    }
    
    setLoading(true);
    setError(null);
    
    console.log('Pokretanje kviza - resetovanje stanja za quizId:', quizId);
    
    try {
      const data = await apiCall(`/quiz-taking/${quizId}/start`, 'POST');
      
      const normalizedData = {
        attemptId: data.AttemptId,
        quizId: data.QuizId,
        quizTitle: data.QuizTitle,
        quizDescription: data.QuizDescription,
        attemptNumber: data.AttemptNumber,
        totalQuestions: data.TotalQuestions,
        startedAt: data.StartedAt,
        questions: data.Questions?.map(q => ({
          questionId: q.QuestionId,
          questionText: q.QuestionText,
          questionType: q.QuestionType,
          difficultyLevel: q.DifficultyLevel,
          options: q.Options || []
        })) || []
      };
      
      console.log('Kviz uspešno pokrenut:', {
        attemptId: normalizedData.attemptId,
        questionsCount: normalizedData.questions.length,
        attemptNumber: normalizedData.attemptNumber
      });
      
      setCurrentAttempt(normalizedData);
      setCurrentQuiz(normalizedData);
      setUserAnswers({});
      setCurrentQuestionIndex(0);
      setView('takingQuiz');
      
    } catch (error) {
      console.error('Greška pri pokretanju kviza:', error);
      setError('Greška pri pokretanju kviza: ' + error.message);
    }
    
    setLoading(false);
  };

  const resetQuizState = () => {
    console.log('Eksplicitno resetovanje stanja kviza');
    
    setCurrentQuiz(null);
    setCurrentAttempt(null);
    setUserAnswers({});
    setCurrentQuestionIndex(0);
    setQuizResult(null);
    setElapsedTime(0);
    setView('starting');
    setError(null);
    setLoading(false);
  };

  useEffect(() => {
    const handleBeforeUnload = () => {
      resetQuizState();
    };
    
    window.addEventListener('beforeunload', handleBeforeUnload);
    
    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, []);

  const submitAnswer = async (questionId, answer) => {
    console.log(`Šalje se odgovor za pitanje ${questionId}:`, answer);
    setLoading(true);
    try {
      const data = await apiCall(
        `/quiz-taking/${currentQuiz.quizId}/${questionId}/answer`,
        'POST',
        { userAnswer: answer }
      );
      
      console.log('Odgovor sa servera:', data);
      
      setUserAnswers(prev => {
        const newAnswers = {
          ...prev,
          [questionId]: {
            ...prev[questionId],
            answer,
            isCorrect: data.IsCorrect || data.isCorrect,
            submitted: true,
            message: data.Message || data.message || 'Odgovor poslat'
          }
        };
        console.log('Novo stanje userAnswers:', newAnswers);
        return newAnswers;
      });
    } catch (error) {
      console.error('Greška pri slanju odgovora:', error);
      setError('Greška pri slanju odgovora: ' + error.message);
    }
    setLoading(false);
  };

  const finishQuiz = async () => {
    if (!currentAttempt?.attemptId) {
      setError('Nema aktivnog pokušaja kviza');
      return;
    }

    setLoading(true);

    try {
      for (const question of currentQuiz.questions) {
        const questionId = question.questionId;
        const answer = userAnswers[questionId]?.answer;
        if (answer !== undefined && answer !== '' && !userAnswers[questionId]?.submitted) {
          await submitAnswer(questionId, answer);
        }
      }

      const data = await apiCall(`/quiz-taking/${currentAttempt.attemptId}/finish`, 'POST');

      const normalizedResult = {
        resultId: data.ResultId,
        quizTitle: data.QuizTitle,
        attemptNumber: data.AttemptNumber,
        totalQuestions: data.TotalQuestions,
        correctAnswers: data.CorrectAnswers,
        scorePercentage: data.ScorePercentage,
        timeTaken: elapsedTime,
        completedAt: data.CompletedAt,
        questionResults: data.QuestionResults?.map(qr => ({
          questionId: qr.QuestionId,
          questionText: qr.QuestionText,
          correctAnswer: qr.CorrectAnswer,
          userAnswer: qr.UserAnswer,
          isCorrect: qr.IsCorrect,
          wasAnswered: qr.WasAnswered
        })) || []
      };

      setQuizResult(normalizedResult);
      setView('results');

    } catch (error) {
      console.error('Greška pri završetku kviza:', error);
      setError('Greška pri završetku kviza: ' + error.message);
    }

    setLoading(false);
  };

  
  const handleAnswerChange = (questionId, answer) => {
    setUserAnswers(prev => ({
      ...prev,
      [questionId]: {
        ...prev[questionId],
        answer,
        submitted: false
      }
    }));
  };

  const handleSubmitAnswer = async (questionId) => {
    const answer = userAnswers[questionId]?.answer;
    if (answer !== undefined && answer !== '') {
      await submitAnswer(questionId, answer);
    }
  };

  const renderQuestionInput = (question) => {
    const questionId = question.questionId;
    const userAnswer = userAnswers[questionId];

    console.log('Question:', question);
    console.log('Options:', question.options);

    switch (question.questionType?.toLowerCase()) {
      case 'true-false':
        return (
          <div className="quiz-options">
            {question.options && question.options.length > 0 ? (
              question.options.map((option, index) => (
                <label key={index} className="quiz-option">
                  <input
                    type="radio"
                    name={`question-${questionId}`}
                    value={option}
                    checked={userAnswer?.answer === option}
                    onChange={(e) => handleAnswerChange(questionId, e.target.value)}
                    disabled={userAnswer?.submitted}
                  />
                  <span>{option}</span>
                </label>
              ))
            ) : (
              ['Tačno', 'Netačno'].map((option, index) => (
                <label key={index} className="quiz-option">
                  <input
                    type="radio"
                    name={`question-${questionId}`}
                    value={option}
                    checked={userAnswer?.answer === option}
                    onChange={(e) => handleAnswerChange(questionId, e.target.value)}
                    disabled={userAnswer?.submitted}
                  />
                  <span>{option}</span>
                </label>
              ))
            )}
          </div>
        );

      case 'multiple-choice':
      case 'one-select':
        return (
          <div className="quiz-options">
            {question.options && question.options.length > 0 ? (
              question.options.map((option, index) => {
                const optionValue = option.charAt(0);
                return (
                  <label key={index} className="quiz-option">
                    <input
                      type="radio"
                      name={`question-${questionId}`}
                      value={optionValue}
                      checked={userAnswer?.answer === optionValue}
                      onChange={(e) => handleAnswerChange(questionId, e.target.value)}
                      disabled={userAnswer?.submitted}
                    />
                    <span>{option}</span>
                  </label>
                );
              })
            ) : (
              <div className="error-message">Nema dostupnih opcija za ovo pitanje</div>
            )}
          </div>
        );

      case 'multi-select':
        return (
          <div className="quiz-options">
            {question.options && question.options.length > 0 ? (
              question.options.map((option, index) => {
                const optionValue = option.charAt(0);
                const selectedAnswers = userAnswer?.answer ? userAnswer.answer.split(',').map(a => a.trim()) : [];
                return (
                  <label key={index} className="quiz-option multi-select">
                    <input
                      type="checkbox"
                      value={optionValue}
                      checked={selectedAnswers.includes(optionValue)}
                      onChange={(e) => {
                        const value = e.target.value;
                        let newAnswers = [...selectedAnswers];
                        if (e.target.checked) {
                          newAnswers.push(value);
                        } else {
                          newAnswers = newAnswers.filter(a => a !== value);
                        }
                        newAnswers.sort();
                        handleAnswerChange(questionId, newAnswers.join(','));
                      }}
                      disabled={userAnswer?.submitted}
                    />
                    <span>{option}</span>
                  </label>
                );
              })
            ) : (
              <div className="error-message">Nema dostupnih opcija za ovo pitanje</div>
            )}
          </div>
        );

      case 'fill-in-the-blank':
        return (
          <input
            type="text"
            value={userAnswer?.answer || ''}
            onChange={(e) => handleAnswerChange(questionId, e.target.value)}
            disabled={userAnswer?.submitted}
            placeholder="Unesite odgovor..."
            className="quiz-text-input"
          />
        );

      default:
        return <div className="error-message">Nepoznat tip pitanja: {question.questionType}</div>;
    }
  };

  if (loading && view === 'starting') {
    return (
      <div className="quiz-container">
        <div className="loading-screen">
          <div className="loading-spinner"></div>
          <p>Pokretanje kviza...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="quiz-container">
        <div className="error-screen">
          <XCircle className="error-icon" />
          <h2>Greška</h2>
          <p>{error}</p>
          <button
            onClick={() => navigate('/quizzes')}
            className="btn btn-primary"
          >
            Povratak na kvizove
          </button>
        </div>
      </div>
    );
  }

  if (view === 'takingQuiz' && currentQuiz) {
    const currentQuestion = currentQuiz.questions?.[currentQuestionIndex];
    const totalQuestions = currentQuiz.questions?.length || 0;
    const answeredCount = Object.keys(userAnswers).filter(id => userAnswers[id]?.submitted).length;

    if (!currentQuestion) {
      return (
        <div className="quiz-container">
          <div className="error-screen">
            <p>Nema dostupnih pitanja za ovaj kviz.</p>
            <button
              onClick={() => { resetQuizState(); navigate('/quizzes')}}
              className="btn btn-primary"
            >
              Povratak na kvizove
            </button>
          </div>
        </div>
      );
    }

    return (
      <div className="quiz-container">
        <div className="quiz-content">
          {/* Header */}
          <header className="quiz-header">
            <div className="quiz-title-section">
              <h1>{currentQuiz.quizTitle}</h1>
              <div className="quiz-meta">
                <span>Pokušaj #{currentQuiz.attemptNumber}</span>
                <span>{answeredCount}/{totalQuestions} odgovoreno</span>
                <div className="time-display">
                  <Clock size={16} />
                  <span>{formatTime(elapsedTime)} / 10:00</span>
                </div>
              </div>
            </div>
            
            <div className="progress-section">
              <div className="progress-bar">
                <div 
                  className="progress-fill"
                  style={{ width: `${totalQuestions > 0 ? (answeredCount / totalQuestions) * 100 : 0}%` }}
                ></div>
              </div>
            </div>
          </header>

          {/* Question */}
          <main className="question-section">
            <div className="question-header">
              <div className="question-counter">
                Pitanje {currentQuestionIndex + 1} od {totalQuestions}
              </div>
              <div className="difficulty-badge">
                {currentQuestion.difficultyLevel || 'N/A'}
              </div>
            </div>
            
            <h2 className="question-text">
              {currentQuestion.questionText}
            </h2>

            <div className="answer-section">
              {renderQuestionInput(currentQuestion)}
            </div>

            <div className="question-actions">
              <div className="answer-status">
                {userAnswers[currentQuestion.questionId]?.submitted && (
                  <>
                    {userAnswers[currentQuestion.questionId]?.isCorrect ? (
                      <CheckCircle className="status-icon correct" />
                    ) : (
                      <XCircle className="status-icon incorrect" />
                    )}
                    <span>
                      {userAnswers[currentQuestion.questionId]?.message || 'Odgovor poslat'}
                    </span>
                  </>
                )}
              </div>

              <button
                onClick={() => handleSubmitAnswer(currentQuestion.questionId)}
                disabled={
                  userAnswers[currentQuestion.questionId]?.answer === undefined || 
                  userAnswers[currentQuestion.questionId]?.answer === '' ||
                  userAnswers[currentQuestion.questionId]?.submitted || 
                  loading
                }
                className="btn btn-submit"
              >
                {loading ? 'Šalje se...' : 'Potvrdi odgovor'}
              </button>
            </div>
          </main>

          {/* Navigation */}
          <nav className="quiz-navigation">
            <button
              onClick={() => setCurrentQuestionIndex(Math.max(0, currentQuestionIndex - 1))}
              disabled={currentQuestionIndex === 0}
              className="btn btn-nav"
            >
              <ArrowLeft size={16} />
              <span>Prethodno</span>
            </button>

            <div className="question-indicators">
              {currentQuiz.questions?.map((_, index) => (
                <button
                  key={index}
                  onClick={() => setCurrentQuestionIndex(index)}
                  className={`question-indicator ${
                    index === currentQuestionIndex ? 'active' : ''
                  } ${userAnswers[currentQuiz.questions[index].questionId]?.submitted ? 'answered' : ''}`}
                >
                  {index + 1}
                </button>
              ))}
            </div>

            {currentQuestionIndex === totalQuestions - 1 ? (
              <button
                onClick={finishQuiz}
                disabled={answeredCount < totalQuestions || loading}
                className="btn btn-finish"
              >
                <Trophy size={16} />
                <span>{loading ? 'Završava se...' : 'Završi kviz'}</span>
              </button>
            ) : (
              <button
                onClick={() => setCurrentQuestionIndex(Math.min(totalQuestions - 1, currentQuestionIndex + 1))}
                disabled={currentQuestionIndex === totalQuestions - 1}
                className="btn btn-nav"
              >
                <span>Sledeće</span>
                <ArrowRight size={16} />
              </button>
            )}
          </nav>
        </div>
      </div>
    );
  }

  if (view === 'results' && quizResult) {
    return (
      <div className="quiz-container">
        <div className="results-content">
          {/* Results Header */}
          <header className="results-header">
            <Trophy className="trophy-icon" />
            <h1>Kviz završen!</h1>
            <h2>{quizResult.quizTitle}</h2>
            
            <div className="results-stats">
              <div className="stat-item">
                <div className="stat-number correct">{quizResult.correctAnswers}</div>
                <div className="stat-label">Tačni odgovori</div>
              </div>
              <div className="stat-item">
                <div className="stat-number total">{quizResult.totalQuestions}</div>
                <div className="stat-label">Ukupno pitanja</div>
              </div>
              <div className="stat-item">
                <div className="stat-number percentage">{quizResult.scorePercentage}%</div>
                <div className="stat-label">Procenat</div>
              </div>
              <div className="stat-item">
                <div className="stat-number time">
                  <Clock size={20} />
                  {Math.floor((quizResult.timeTaken || 0) / 60)}:{String((quizResult.timeTaken || 0) % 60).padStart(2, '0')}
                </div>
                <div className="stat-label">Vreme</div>
              </div>
            </div>
          </header>

          {/* Question Results */}
          <section className="results-details">
            <h3>Detalji odgovora</h3>
            <div className="question-results">
              {quizResult.questionResults?.map((result, index) => (
                <div key={result.questionId} className="result-item">
                  <div className="result-header">
                    <h4>
                      <span className="question-number">{index + 1}.</span> 
                      {result.questionText}
                    </h4>
                    {result.isCorrect ? (
                      <CheckCircle className="result-icon correct" />
                    ) : (
                      <XCircle className="result-icon incorrect" />
                    )}
                  </div>
                  
                  <div className="result-details">
                    <div className="answer-row">
                      <span className="answer-label">Vaš odgovor:</span>
                      <span className={`answer-value ${result.isCorrect ? 'correct' : 'incorrect'}`}>
                        {result.userAnswer || 'Nema odgovora'}
                      </span>
                    </div>
                    {!result.isCorrect && result.correctAnswer && (
                      <div className="answer-row">
                        <span className="answer-label">Tačan odgovor:</span>
                        <span className="answer-value correct">{result.correctAnswer}</span>
                      </div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </section>

          {/* Actions */}
          <footer className="results-actions">
            <button
              onClick={() => navigate('/quizzes')}
              className="btn btn-primary"
            >
              <BookOpen size={18} />
              <span>Povratak na kvizove</span>
            </button>
            
            <button
              onClick={() => {
                setView('starting');
                startQuiz();
              }}
              className="btn btn-secondary"
            >
              <ArrowRight size={18} />
              <span>Ponovi kviz</span>
            </button>
          </footer>
        </div>
      </div>
    );
  }

  return null;
};

export default QuizTaking;