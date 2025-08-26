import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import Login from './components/Auth/Login';
import Quizzes from './components/Quiz/Quizzes';
import Register from './components/Auth/Register';
import ViewProfile from './components/Profile/ViewProfile';
import EditProfile from './components/Profile/EditProfile';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/quizzes" element={<Quizzes />} />
        <Route path="/profile" element={<ViewProfile />} />
        <Route path="/profile/edit" element={<EditProfile />} />
      </Routes>
    </Router>
  );
}

export default App;
