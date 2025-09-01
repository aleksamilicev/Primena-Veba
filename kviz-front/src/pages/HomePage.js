import React from 'react';
import Navbar from '../components/Layout/Navbar';
import '../styles/HomePage.css';

const HomePage = () => {
  return (
    <div className="homepage">
      <Navbar />
      <div className="homepage-background">
        <div className="gradient-orb orb-1"></div>
        <div className="gradient-orb orb-2"></div>
        <div className="gradient-orb orb-3"></div>
        <div className="gradient-orb orb-4"></div>
      </div>
      
      <main className="homepage-content">
        <div className="hero-section">
          <div className="hero-text">
            <h1 className="hero-title">
              <span className="title-highlight">Dobrodošli</span>
              <span className="title-main">u Kviz Hub</span>
              <div className="title-decoration">
                <span className="decoration-icon">🎯</span>
                <span className="decoration-line"></span>
                <span className="decoration-icon">✨</span>
              </div>
            </h1>
            
            <p className="hero-subtitle">
              Testirajte svoje znanje kroz zabavne i izazovne kvizove.
              <br />
              Učite, takmičite se i otkrivajte nova znanja!
            </p>
            
            <div className="hero-features">
              <div className="feature-item">
                <span className="feature-icon">📚</span>
                <span className="feature-text">Raznovrsni kvizovi</span>
              </div>
              <div className="feature-item">
                <span className="feature-icon">🏆</span>
                <span className="feature-text">Praćenje rezultata</span>
              </div>
              <div className="feature-item">
                <span className="feature-icon">⚡</span>
                <span className="feature-text">Trenutni feedback</span>
              </div>
            </div>
          </div>
          
          <div className="hero-visual">
            <div className="floating-elements">
              <div className="floating-card card-1">
                <div className="card-icon">🧠</div>
                <div className="card-text">Inteligentno</div>
              </div>
              <div className="floating-card card-2">
                <div className="card-icon">🎮</div>
                <div className="card-text">Zabavno</div>
              </div>
              <div className="floating-card card-3">
                <div className="card-icon">📊</div>
                <div className="card-text">Analitično</div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default HomePage;