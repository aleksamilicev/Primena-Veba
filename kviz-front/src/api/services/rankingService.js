import axios from "axios";
import { RankingEntry } from "../models/RankingEntry";

const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

export async function fetchRankings(period = "all", quizId = "") {
  const token = localStorage.getItem("token");
  try {
    let url = `${API_BASE_URL}/ranking?period=${period}`;
    if (quizId.trim() !== "") {
      url = `${API_BASE_URL}/ranking/${quizId}?period=${period}`;
    }

    const response = await axios.get(url, {
      headers: {
        Authorization: token ? `Bearer ${token}` : "",
      },
    });

    return response.data.map((r) => new RankingEntry(r));
  } catch (err) {
    console.error("Failed to fetch rankings:", err);
    throw err;
  }
}
