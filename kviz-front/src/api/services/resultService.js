import axios from "axios";

const API_URL = process.env.REACT_APP_API_BASE_URL;

export const fetchAllResults = async (page, pageSize) => {
  const token = localStorage.getItem("token");
  const res = await axios.get(
    `${API_URL}/quizresults/admin/all-results?page=${page}&pageSize=${pageSize}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  return res.data;
};

export const fetchMyResults = async (page, pageSize) => {
  const token = localStorage.getItem("token");
  const res = await axios.get(
    `${API_URL}/quizresults/my-results?page=${page}&pageSize=${pageSize}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  return res.data;
};

export async function fetchDetailedResult(resultId) {
  const token = localStorage.getItem("token");
  const res = await axios.get(
    `${API_URL}/quizresults/${resultId}/details`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  return res.data;
}
