import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { BookOpen, Clock, CheckCircle, XCircle, ArrowRight, ArrowLeft, Trophy } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

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
  const [view, setView] = useState('starting'); // 'starting', 'takingQuiz', 'results'
  const [error, setError] = useState(null);

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
    if (quizId && !currentQuiz) {
      startQuiz();
    }
  }, [quizId]);

  const startQuiz = async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await apiCall(`/quiz-taking/${quizId}/start`, 'POST');
      
      // Konvertuj backend podatke u format koji frontend očekuje
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

  const submitAnswer = async (questionId, answer) => {
    setLoading(true);
    try {
      const data = await apiCall(
        `/quiz-taking/${currentQuiz.quizId}/${questionId}/answer`,
        'POST',
        { userAnswer: answer }
      );
      
      setUserAnswers(prev => ({
        ...prev,
        [questionId]: {
          answer,
          isCorrect: data.IsCorrect || data.isCorrect,
          submitted: true,
          message: data.Message || data.message || 'Odgovor poslat'
        }
      }));
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
      const data = await apiCall(`/quiz-taking/${currentAttempt.attemptId}/finish`, 'POST');
      
      // Normalizuj podatke za rezultate
      const normalizedResult = {
        resultId: data.ResultId,
        quizTitle: data.QuizTitle,
        attemptNumber: data.AttemptNumber,
        totalQuestions: data.TotalQuestions,
        correctAnswers: data.CorrectAnswers,
        scorePercentage: data.ScorePercentage,
        timeTaken: data.TimeTaken,
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

  // Debug log da vidiš šta stiže
  console.log('Question:', question);
  console.log('Options:', question.options);

  switch (question.questionType?.toLowerCase()) {
    case 'true-false':
      return (
        <div className="space-y-3">
          {question.options && question.options.length > 0 ? (
            question.options.map((option, index) => (
              <label key={index} className="flex items-center space-x-3 p-3 rounded-lg border hover:bg-gray-50 cursor-pointer">
                <input
                  type="radio"
                  name={`question-${questionId}`}
                  value={option}
                  checked={userAnswer?.answer === option}
                  onChange={(e) => handleAnswerChange(questionId, e.target.value)}
                  disabled={userAnswer?.submitted}
                  className="text-blue-600 focus:ring-blue-500"
                />
                <span className="text-lg">{option}</span>
              </label>
            ))
          ) : (
            // Fallback ako opcije ne stižu
            ['Tačno', 'Netačno'].map((option, index) => (
              <label key={index} className="flex items-center space-x-3 p-3 rounded-lg border hover:bg-gray-50 cursor-pointer">
                <input
                  type="radio"
                  name={`question-${questionId}`}
                  value={option}
                  checked={userAnswer?.answer === option}
                  onChange={(e) => handleAnswerChange(questionId, e.target.value)}
                  disabled={userAnswer?.submitted}
                  className="text-blue-600 focus:ring-blue-500"
                />
                <span className="text-lg">{option}</span>
              </label>
            ))
          )}
        </div>
      );

    case 'multiple-choice':
    case 'one-select':
      return (
        <div className="space-y-3">
          {question.options && question.options.length > 0 ? (
            question.options.map((option, index) => {
              // Za "A) Java" izvuci "A"
              const optionValue = option.charAt(0);
              return (
                <label key={index} className="flex items-center space-x-3 p-3 rounded-lg border hover:bg-gray-50 cursor-pointer">
                  <input
                    type="radio"
                    name={`question-${questionId}`}
                    value={optionValue}
                    checked={userAnswer?.answer === optionValue}
                    onChange={(e) => handleAnswerChange(questionId, e.target.value)}
                    disabled={userAnswer?.submitted}
                    className="text-blue-600 focus:ring-blue-500"
                  />
                  <span className="text-lg">{option}</span>
                </label>
              );
            })
          ) : (
            <div className="text-red-500">Nema dostupnih opcija za ovo pitanje</div>
          )}
        </div>
      );

    case 'multi-select':
      return (
        <div className="space-y-3">
          {question.options && question.options.length > 0 ? (
            question.options.map((option, index) => {
              // Za "A) Java" izvuci "A"
              const optionValue = option.charAt(0);
              const selectedAnswers = userAnswer?.answer ? userAnswer.answer.split(',').map(a => a.trim()) : [];
              return (
                <label key={index} className="flex items-center space-x-3 p-3 rounded-lg border hover:bg-gray-50 cursor-pointer">
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
                      // Sortiraj odgovore da budu konzistentni
                      newAnswers.sort();
                      handleAnswerChange(questionId, newAnswers.join(','));
                    }}
                    disabled={userAnswer?.submitted}
                    className="text-blue-600 focus:ring-blue-500"
                  />
                  <span className="text-lg">{option}</span>
                </label>
              );
            })
          ) : (
            <div className="text-red-500">Nema dostupnih opcija za ovo pitanje</div>
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
          className="w-full px-4 py-3 text-lg border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
        />
      );

    default:
      return <div className="text-red-500">Nepoznat tip pitanja: {question.questionType}</div>;
  }
};

// Dodaj debug funkciju da vidiš šta tačno stiže sa servera
const debugQuestionData = (quizData) => {
  console.log('Quiz data received:', quizData);
  if (quizData.questions) {
    quizData.questions.forEach((q, index) => {
      console.log(`Question ${index + 1}:`, {
        id: q.questionId,
        type: q.questionType,
        text: q.questionText,
        options: q.options
      });
    });
  }
};

  if (loading && view === 'starting') {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Pokretanje kviza...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center max-w-md mx-auto">
          <XCircle className="h-16 w-16 text-red-500 mx-auto mb-4" />
          <h2 className="text-2xl font-bold text-gray-900 mb-2">Greška</h2>
          <p className="text-gray-600 mb-4">{error}</p>
          <button
            onClick={() => navigate('/quizzes')}
            className="bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition-colors"
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
        <div className="min-h-screen bg-gray-50 flex items-center justify-center">
          <div className="text-center">
            <p className="text-gray-600">Nema dostupnih pitanja za ovaj kviz.</p>
            <button
              onClick={() => navigate('/quizzes')}
              className="mt-4 bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition-colors"
            >
              Povratak na kvizove
            </button>
          </div>
        </div>
      );
    }

    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-4xl mx-auto px-4">
          {/* Header */}
          <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
            <div className="flex justify-between items-center mb-4">
              <h1 className="text-2xl font-bold text-gray-900">{currentQuiz.quizTitle}</h1>
              <div className="flex items-center space-x-4 text-sm text-gray-600">
                <span>Pokušaj #{currentQuiz.attemptNumber}</span>
                <span>{answeredCount}/{totalQuestions} odgovoreno</span>
              </div>
            </div>
            
            {/* Progress bar */}
            <div className="w-full bg-gray-200 rounded-full h-3">
              <div 
                className="bg-blue-600 h-3 rounded-full transition-all duration-300"
                style={{ width: `${totalQuestions > 0 ? (answeredCount / totalQuestions) * 100 : 0}%` }}
              ></div>
            </div>
          </div>

          {/* Question */}
          <div className="bg-white rounded-lg shadow-sm p-6 mb-6">
            <div className="mb-6">
              <div className="flex justify-between items-center mb-4">
                <span className="text-sm text-gray-500">
                  Pitanje {currentQuestionIndex + 1} od {totalQuestions}
                </span>
                <span className="text-sm px-3 py-1 bg-blue-100 text-blue-800 rounded-full">
                  {currentQuestion.difficultyLevel || 'N/A'}
                </span>
              </div>
              <h2 className="text-xl font-semibold text-gray-900 mb-6">
                {currentQuestion.questionText}
              </h2>
            </div>

            <div className="mb-6">
              {renderQuestionInput(currentQuestion)}
            </div>

            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-3">
                {userAnswers[currentQuestion.questionId]?.submitted && (
                  <>
                    {userAnswers[currentQuestion.questionId]?.isCorrect ? (
                      <CheckCircle className="h-6 w-6 text-green-600" />
                    ) : (
                      <XCircle className="h-6 w-6 text-red-600" />
                    )}
                    <span className="text-sm text-gray-600">
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
                className="bg-green-600 text-white py-2 px-6 rounded-md hover:bg-green-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
              >
                {loading ? 'Šalje se...' : 'Potvrdi odgovor'}
              </button>
            </div>
          </div>

          {/* Navigation */}
          <div className="flex justify-between items-center">
            <button
              onClick={() => setCurrentQuestionIndex(Math.max(0, currentQuestionIndex - 1))}
              disabled={currentQuestionIndex === 0}
              className="flex items-center space-x-2 py-2 px-4 text-gray-600 hover:text-gray-800 disabled:text-gray-400 disabled:cursor-not-allowed transition-colors"
            >
              <ArrowLeft className="h-4 w-4" />
              <span>Prethodno pitanje</span>
            </button>

            <div className="flex space-x-2">
              {currentQuiz.questions?.map((_, index) => (
                <button
                  key={index}
                  onClick={() => setCurrentQuestionIndex(index)}
                  className={`w-10 h-10 rounded-full text-sm font-medium transition-colors ${
                    index === currentQuestionIndex
                      ? 'bg-blue-600 text-white'
                      : userAnswers[currentQuiz.questions[index].questionId]?.submitted
                      ? 'bg-green-200 text-green-800 hover:bg-green-300'
                      : 'bg-gray-200 text-gray-600 hover:bg-gray-300'
                  }`}
                >
                  {index + 1}
                </button>
              ))}
            </div>

            {currentQuestionIndex === totalQuestions - 1 ? (
              <button
                onClick={finishQuiz}
                disabled={answeredCount < totalQuestions || loading}
                className="bg-red-600 text-white py-2 px-6 rounded-md hover:bg-red-700 disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors flex items-center space-x-2"
              >
                <Trophy className="h-4 w-4" />
                <span>{loading ? 'Završava se...' : 'Završi kviz'}</span>
              </button>
            ) : (
              <button
                onClick={() => setCurrentQuestionIndex(Math.min(totalQuestions - 1, currentQuestionIndex + 1))}
                disabled={currentQuestionIndex === totalQuestions - 1}
                className="flex items-center space-x-2 py-2 px-4 text-gray-600 hover:text-gray-800 disabled:text-gray-400 disabled:cursor-not-allowed transition-colors"
              >
                <span>Sledeće pitanje</span>
                <ArrowRight className="h-4 w-4" />
              </button>
            )}
          </div>
        </div>
      </div>
    );
  }

  if (view === 'results' && quizResult) {
    return (
      <div className="min-h-screen bg-gray-50 py-8">
        <div className="max-w-4xl mx-auto px-4">
          {/* Results Header */}
          <div className="bg-white rounded-lg shadow-sm p-8 mb-6 text-center">
            <Trophy className="h-20 w-20 text-yellow-500 mx-auto mb-4" />
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Kviz završen!</h1>
            <h2 className="text-xl text-gray-700 mb-6">{quizResult.quizTitle}</h2>
            
            <div className="grid grid-cols-2 md:grid-cols-4 gap-6 mt-8">
              <div className="text-center">
                <div className="text-3xl font-bold text-blue-600">{quizResult.correctAnswers}</div>
                <div className="text-sm text-gray-600 mt-1">Tačni odgovori</div>
              </div>
              <div className="text-center">
                <div className="text-3xl font-bold text-gray-800">{quizResult.totalQuestions}</div>
                <div className="text-sm text-gray-600 mt-1">Ukupno pitanja</div>
              </div>
              <div className="text-center">
                <div className="text-3xl font-bold text-green-600">{quizResult.scorePercentage}%</div>
                <div className="text-sm text-gray-600 mt-1">Procenat</div>
              </div>
              <div className="text-center">
                <div className="text-3xl font-bold text-purple-600 flex items-center justify-center">
                  <Clock className="h-6 w-6 mr-2" />
                  {Math.floor((quizResult.timeTaken || 0) / 60)}:{String((quizResult.timeTaken || 0) % 60).padStart(2, '0')}
                </div>
                <div className="text-sm text-gray-600 mt-1">Vreme</div>
              </div>
            </div>
          </div>

          {/* Question Results */}
          <div className="space-y-4 mb-8">
            <h3 className="text-xl font-semibold text-gray-900">Detalji odgovora</h3>
            {quizResult.questionResults?.map((result, index) => (
              <div key={result.questionId} className="bg-white rounded-lg shadow-sm p-6">
                <div className="flex items-start justify-between mb-4">
                  <h4 className="font-medium text-gray-900 flex-1 pr-4">
                    <span className="text-blue-600 font-bold">{index + 1}.</span> {result.questionText}
                  </h4>
                  {result.isCorrect ? (
                    <CheckCircle className="h-7 w-7 text-green-600 flex-shrink-0" />
                  ) : (
                    <XCircle className="h-7 w-7 text-red-600 flex-shrink-0" />
                  )}
                </div>
                
                <div className="space-y-2 text-sm">
                  <div className="flex items-start">
                    <span className="font-medium text-gray-700 w-20">Vaš odgovor:</span>
                    <span className={`font-medium ${result.isCorrect ? 'text-green-600' : 'text-red-600'}`}>
                      {result.userAnswer || 'Nema odgovora'}
                    </span>
                  </div>
                  {!result.isCorrect && result.correctAnswer && (
                    <div className="flex items-start">
                      <span className="font-medium text-gray-700 w-20">Tačan odgovor:</span>
                      <span className="text-green-600 font-medium">{result.correctAnswer}</span>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>

          {/* Actions */}
          <div className="text-center space-x-4">
            <button
              onClick={() => navigate('/quizzes')}
              className="bg-blue-600 text-white py-3 px-8 rounded-md hover:bg-blue-700 transition-colors inline-flex items-center space-x-2"
            >
              <BookOpen className="h-5 w-5" />
              <span>Povratak na kvizove</span>
            </button>
            
            <button
              onClick={() => window.location.reload()}
              className="bg-green-600 text-white py-3 px-8 rounded-md hover:bg-green-700 transition-colors inline-flex items-center space-x-2"
            >
              <ArrowRight className="h-5 w-5" />
              <span>Ponovi kviz</span>
            </button>
          </div>
        </div>
      </div>
    );
  }

  return null;
};

export default QuizTaking;