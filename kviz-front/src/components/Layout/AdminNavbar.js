// src/components/Navbar.js
import { Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

function Navbar() {
  const { user } = useAuth();

  return (
    <nav>
      <Link to="/">Home</Link>
      <Link to="/quizzes">Quizzes</Link>
      {user?.role === "admin" && (
        <>
          <Link to="/admin/quizzes">Manage Quizzes</Link>
          <Link to="/admin/quizzes/create">Create Quiz</Link>
        </>
      )}
    </nav>
  );
}

export default Navbar;
