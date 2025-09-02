import React from 'react';
import { Link } from 'react-router-dom';
import '../../styles/Navbar.css';

const Navbar = () => {
  const user = JSON.parse(localStorage.getItem('user'));
  
  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/';
  };

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <Link to="/" className="navbar-brand">
          <span className="brand-icon">🎯</span>
          Kviz Hub
        </Link>
        
        <div className="navbar-menu">
          {user ? (
            <div className="user-section">
              <Link to="/quizzes" className="nav-link">
                📚 Quizzes
              </Link>
              <span className="welcome-message">
                Zdravo, <span className="username">{user.username}</span>
              </span>
              <button onClick={handleLogout} className="logout-btn">
                <span className="btn-icon">👋</span>
                Logout
              </button>
            </div>
          ) : (
            <div className="auth-links">
              <Link to="/login" className="nav-link login-link">
                <span className="link-icon">🔑</span>
                Login
              </Link>
              <Link to="/register" className="nav-link register-link">
                <span className="link-icon">✨</span>
                Register
              </Link>
            </div>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;