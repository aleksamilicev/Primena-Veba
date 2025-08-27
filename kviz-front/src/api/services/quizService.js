// src/api/services/quizService.js
import axios from "axios";

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

// Dobavljanje tokena iz localStorage
const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  return {
    Authorization: `Bearer ${token}`,
    "Content-Type": "application/json",
  };
};

// GET svi kvizovi
export const getQuizzes = async () => {
  try {
    const response = await axios.get(`${API_BASE_URL}/quizzes`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error fetching quizzes:", error);
    throw error;
  }
};

// GET kviz po ID-u
export const getQuizById = async (id) => {
  try {
    const response = await axios.get(`${API_BASE_URL}/quizzes/${id}`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error fetching quiz by ID:", error);
    throw error;
  }
};

// POST kreiranje novog kviza (admin)
export const createQuiz = async (quizData) => {
  try {
    const response = await axios.post(`${API_BASE_URL}/quizzes`, quizData, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error creating quiz:", error);
    throw error;
  }
};

// PUT update kviza
export const updateQuiz = async (id, quizData) => {
  try {
    const response = await axios.put(`${API_BASE_URL}/quizzes/${id}`, quizData, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error updating quiz:", error);
    throw error;
  }
};

// DELETE kviza
export const deleteQuiz = async (id) => {
  try {
    const response = await axios.delete(`${API_BASE_URL}/quizzes/${id}`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error deleting quiz:", error);
    throw error;
  }
};

// POST dodavanje pitanja u kviz (admin)
export const addQuestionToQuiz = async (quizId, questionData) => {
  try {
    const payload = {
      quizId: parseInt(quizId),
      questionText: questionData.questionText,
      questionType: questionData.questionType,
      difficultyLevel: questionData.difficultyLevel,
      correctAnswer: questionData.correctAnswer,
    };

    console.log("Sending to backend:", payload);

    const response = await axios.post(
      `${API_BASE_URL}/questions/quiz/${quizId}`,
      payload,
      {
        headers: getAuthHeaders(),
      }
    );

    return response.data;
  } catch (error) {
    console.error("Error adding question:", error);
    throw error;
  }
};

// GET pitanja za kviz po quizId (admin)
export const getQuestionsByQuizId = async (quizId) => {
  try {
    const response = await axios.get(`${API_BASE_URL}/questions/quiz/${quizId}/questions`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error fetching questions:", error);
    throw error;
  }
};

// DELETE kviza po ID-u (admin)
export const deleteQuizById = async (quizId) => {
  try {
    const response = await axios.delete(`${API_BASE_URL}/quizzes/${quizId}`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error deleting quiz:", error);
    throw error;
  }
};

// UPDATE kviza po ID-u (admin)
export const updateQuizById = async (quizId, quizData) => {
  try {
    const response = await axios.put(`${API_BASE_URL}/quizzes/${quizId}`, quizData, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error updating quiz:", error);
    throw error;
  }
};

// GET pojedinačno pitanje po ID
export const getQuestionById = async (questionId) => {
  try {
    const response = await axios.get(`${API_BASE_URL}/questions/question/${questionId}`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error fetching question:", error);
    throw error;
  }
};

// DELETE pitanja po ID-u (admin)
export const deleteQuestionById = async (questionId) => {
  try {
    const response = await axios.delete(`${API_BASE_URL}/questions/question/${questionId}`, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error deleting question:", error);
    throw error;
  }
};

// UPDATE pitanja po ID-u (ispravljen)
export const updateQuestionById = async (questionId, questionData) => {
  try {
    // Mapiranje frontend naziva na backend naziva
    const payload = {
      questionText: questionData.questionText,
      questionType: questionData.questionType,
      difficultyLevel: questionData.difficultyLevel,
      correctAnswer: questionData.correctAnswer
    };

    const response = await axios.put(`${API_BASE_URL}/questions/question/${questionId}`, payload, {
      headers: getAuthHeaders(),
    });
    return response.data;
  } catch (error) {
    console.error("Error updating question:", error);
    throw error;
  }
};