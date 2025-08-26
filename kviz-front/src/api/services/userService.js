import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

export const loginUser = async (username, password, email) => {
  const response = await axios.post(`${API_BASE_URL}/users/login`, {
    username,
    password,
    email
  });
  return response.data; // očekuješ token
};

export const registerUser = async (username, password, email, profileImageUrl) => {
  const response = await axios.post(`${API_BASE_URL}/users/register`, {
    username,
    password,
    email,
    profileImageUrl
  });
  return response.data; // očekuješ token
};

export const getProfile = async () => {
  const token = localStorage.getItem("token");
  const response = await axios.get(`${API_BASE_URL}/users/profile`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return response.data;
};

export const updateProfile = async (updateData) => {
  const token = localStorage.getItem("token");
  const response = await axios.put(`${API_BASE_URL}/users/profile/edit`, updateData, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return response.data;
};