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
