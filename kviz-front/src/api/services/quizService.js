// src/api/services/quizService.js
import axios from "axios";

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

export const getQuizzes = async () => {
  try {
    const token = localStorage.getItem("token");
    const response = await axios.get(`${API_BASE_URL}/quizzes`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    return response.data;
  } catch (error) {
    console.error("Error fetching quizzes:", error);
    throw error;
  }
};

export const getQuizById = async (id) => {
  const res = await axios.get(`${API_BASE_URL}/quizzes/${id}`);
  return res.data;
};

export const createQuiz = async (quizData) => {
  const res = await axios.post(`${API_BASE_URL}/quizzes`, quizData);
  return res.data;
};

export const updateQuiz = async (id, quizData) => {
  const res = await axios.put(`${API_BASE_URL}/quizzes/${id}`, quizData);
  return res.data;
};

export const deleteQuiz = async (id) => {
  const res = await axios.delete(`${API_BASE_URL}/quizzes/${id}`);
  return res.data;
};
