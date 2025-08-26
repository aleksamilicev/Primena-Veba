import React, { useState } from "react";
import User from "../../api/models/User";
import { loginUser } from "../../api/services/userService";
import { useNavigate } from "react-router-dom";

const Login = () => {
  const [usernameOrEmail, setUsernameOrEmail] = useState(""); // Polje za username ili email
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();

    // Ako je uneseno ime, koristimo ga kao username
    // Ako je unesena email adresa, koristimo je kao email
    const isEmail = usernameOrEmail.includes('@');
    
    const user = {
      username: isEmail ? "" : usernameOrEmail, // Ako je email, ostavi username prazan
      email: isEmail ? usernameOrEmail : "", // Ako je username, ostavi email prazan
      password,
    };

    console.log("Sending login request:", user);

    try {
      const result = await loginUser(user.username, user.password, user.email);
      console.log("Login response:", result);

      // Čuvanje tokena i korisničkih podataka
      localStorage.setItem("token", result.token);
      localStorage.setItem(
        "user",
        JSON.stringify({
          userId: result.userId,
          username: result.username,
          email: result.email,
          isAdmin: result.isAdmin,
        })
      );

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
