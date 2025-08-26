// src/components/Auth/Register.js
import React, { useState } from "react";
import User from "../../api/models/User";
import { registerUser } from "../../api/services/userService";
import { useNavigate } from "react-router-dom";

const Register = () => {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [profileImageUrl, setProfileImageUrl] = useState("");
  const [message, setMessage] = useState("");
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    try {
      const newUser = new User({ username, email, password, profileImageUrl: profileImageUrl });
      await registerUser(newUser.username, newUser.password, newUser.email, newUser.profileImageUrl);

      alert("Registration successful!");
      navigate("/login");
    } catch (error) {
      console.error("Registration error:", error.response || error);
      setMessage("Registration failed. Try again.");
    }
  };

  return (
    <div>
      <h2>Register</h2>
      <form onSubmit={handleRegister}>
        <input
          type="text"
          placeholder="Username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          required
        />
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <input
          type="text"
          placeholder="Profile Image URL"
          value={profileImageUrl}
          onChange={(e) => setProfileImageUrl(e.target.value)}
        />
        <button type="submit">Register</button>
      </form>
      <p>{message}</p>
    </div>
  );
};

export default Register;
