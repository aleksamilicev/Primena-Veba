import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import Login from './components/Auth/Login';
import Quizzes from './components/Quiz/Quizzes';
import Register from './components/Auth/Register';
import ViewProfile from './components/Profile/ViewProfile';
import EditProfile from './components/Profile/EditProfile';
import CreateQuiz from './components/Quiz/CreateQuiz';
import AddQuestion from './components/Quiz/AddQuestion';
import QuizQuestions from './components/Quiz/QuizQuestions';
import EditQuiz from './components/Quiz/EditQuiz';
import EditQuestion from './components/Quiz/EditQuestion';

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
        <Route path="/admin/quizzes/create" element={<CreateQuiz />} />
        <Route path="/admin/quizzes/:quizId/add-question" element={<AddQuestion />} />
        <Route path="/admin/quizzes/:quizId/questions" element={<QuizQuestions />} />
        <Route path="/admin/quizzes/:quizId/edit" element={<EditQuiz />} />
        <Route path="/admin/questions/:questionId/edit" element={<EditQuestion />} />
      </Routes>
    </Router>
  );
}

export default App;
