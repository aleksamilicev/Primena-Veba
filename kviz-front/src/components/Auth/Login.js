import React, { useState } from "react";
//import User from "../../api/models/User";
import { loginUser } from "../../api/services/userService";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";

const Login = () => {
  const [usernameOrEmail, setUsernameOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleLogin = async (e) => {
    e.preventDefault();

    const isEmail = usernameOrEmail.includes('@');
    const username = isEmail ? "" : usernameOrEmail;
    const email = isEmail ? usernameOrEmail : "";

    try {
      const result = await loginUser(username, password, email);
      console.log("Login response:", result);

      // Umesto da ovde stavljaš u localStorage, koristi login iz context-a
      login(result.token);

      alert("Login successful!");
      navigate("/quizzes");
    } catch (error) {
      console.error("Login error:", error.response || error);
      setMessage("Login failed. Check credentials.");
    }
  };

  return (
    <div>
      <h2>Login</h2>
      <form onSubmit={handleLogin}>
        <input
          type="text"
          placeholder="Username or Email"
          value={usernameOrEmail}
          onChange={(e) => setUsernameOrEmail(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <button type="submit">Login</button>
      </form>
      <p>{message}</p>
    </div>
  );
};

export default Login;
