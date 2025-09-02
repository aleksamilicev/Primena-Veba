import React, { useState } from "react";
import User from "../../api/models/User";
import { registerUser } from "../../api/services/userService";
import { useNavigate } from "react-router-dom";
import "../../styles/Register.css";

const Register = () => {
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [profileImageUrl, setProfileImageUrl] = useState("");
  const [message, setMessage] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const handleRegister = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage("");

    try {
      const newUser = new User({ 
        username, 
        email, 
        password, 
        profileImageUrl: profileImageUrl 
      });
      
      await registerUser(
        newUser.username, 
        newUser.password, 
        newUser.email, 
        newUser.profileImageUrl
      );
      
      setMessage("Uspešno ste se registrovali!");
      
      setTimeout(() => {
        navigate("/login");
      }, 1500);
    } catch (error) {
      console.error("Registration error:", error.response || error);
      setMessage("Registracija nije uspešna. Pokušajte ponovo.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="register-container">
      <div className="register-background">
        <div className="gradient-orb orb-1"></div>
        <div className="gradient-orb orb-2"></div>
        <div className="gradient-orb orb-3"></div>
        <div className="gradient-orb orb-4"></div>
      </div>
      
      <div className="register-card">
        <div className="register-header">
          <h1 className="register-title">Pridružite se</h1>
          <p className="register-subtitle">Kreirajte vaš nalog i započnite svoje putovanje</p>
        </div>

        <form onSubmit={handleRegister} className="register-form">
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Korisničko ime</label>
              <input
                type="text"
                className="form-input"
                placeholder="Izaberite korisničko ime"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                disabled={isLoading}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Email adresa</label>
              <input
                type="email"
                className="form-input"
                placeholder="Unesite vašu email adresu"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={isLoading}
              />
            </div>
          </div>

          <div className="form-group">
            <label className="form-label">Lozinka</label>
            <input
              type="password"
              className="form-input"
              placeholder="Kreirajte sigurnu lozinku"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              disabled={isLoading}
            />
          </div>

          <div className="form-group">
            <label className="form-label">Slika profila (opciono)</label>
            <input
              type="url"
              className="form-input"
              placeholder="URL vaše profilne slike"
              value={profileImageUrl}
              onChange={(e) => setProfileImageUrl(e.target.value)}
              disabled={isLoading}
            />
          </div>

          <button 
            type="submit" 
            className="register-button"
            disabled={isLoading}
          >
            {isLoading ? (
              <span className="button-content">
                <span className="loading-spinner"></span>
                Kreiranje naloga...
              </span>
            ) : (
              <span className="button-content">
                <span className="btn-icon">✨</span>
                Registruj se
              </span>
            )}
          </button>

          {message && (
            <div className={`message ${message.includes('nije uspešna') ? 'error' : 'success'}`}>
              {message}
            </div>
          )}
        </form>

        <div className="register-footer">
          <p>Već imate nalog? <a href="/login" className="login-link">Prijavite se</a></p>
        </div>
      </div>
    </div>
  );
};

export default Register;