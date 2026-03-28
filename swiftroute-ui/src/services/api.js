import axios from "axios";

const api = axios.create({
  baseURL: "http://localhost:5059", // your backend URL
});

export default api;