import React, { useState } from "react";
import { loginUser } from "../../api/services/userService";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import "../../styles/Login.css";

const Login = () => {
  const [usernameOrEmail, setUsernameOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleLogin = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage("");

    const isEmail = usernameOrEmail.includes('@');
    const username = isEmail ? "" : usernameOrEmail;
    const email = isEmail ? usernameOrEmail : "";

    try {
      const result = await loginUser(username, password, email);
      console.log("Login response:", result);
      
      login(result.token);
      setMessage("Uspešno ste se prijavili!");
      
      setTimeout(() => {
        navigate("/quizzes");
      }, 1000);
    } catch (error) {
      console.error("Login error:", error.response || error);
      setMessage("Neuspešna prijava. Proverite podatke.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-background">
        <div className="gradient-orb orb-1"></div>
        <div className="gradient-orb orb-2"></div>
        <div className="gradient-orb orb-3"></div>
      </div>
      
      <div className="login-card">
        <div className="login-header">
          <h1 className="login-title">Dobrodošli</h1>
          <p className="login-subtitle">Prijavite se na vaš nalog</p>
        </div>

        <form onSubmit={handleLogin} className="login-form">
          <div className="form-group">
            <label className="form-label">Email ili korisničko ime</label>
            <input
              type="text"
              className="form-input"
              placeholder="Unesite email ili korisničko ime"
              value={usernameOrEmail}
              onChange={(e) => setUsernameOrEmail(e.target.value)}
              required
              disabled={isLoading}
            />
          </div>

          <div className="form-group">
            <label className="form-label">Lozinka</label>
            <input
              type="password"
              className="form-input"
              placeholder="Unesite lozinku"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={isLoading}
            />
          </div>

          <button 
            type="submit" 
            className="login-button"
            disabled={isLoading}
          >
            {isLoading ? (
              <span className="button-content">
                <span className="loading-spinner"></span>
                Prijavljivanje...
              </span>
            ) : (
              "Prijavite se"
            )}
          </button>

          {message && (
            <div className={`message ${message.includes('Neuspešna') ? 'error' : 'success'}`}>
              {message}
            </div>
          )}
        </form>

        <div className="login-footer">
          <p>Nemate nalog? <a href="/register" className="register-link">Registrujte se</a></p>
        </div>
      </div>
    </div>
  );
};

export default Login;