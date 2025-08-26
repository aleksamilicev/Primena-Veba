import React from 'react';
import { Link } from 'react-router-dom';

const Navbar = () => {
  const user = JSON.parse(localStorage.getItem('user'));

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.href = '/'; // refresh i redirect na home
  };

  return (
    <nav style={{ display: 'flex', justifyContent: 'flex-end', padding: '10px' }}>
      <Link to="/" style={{ marginRight: '20px' }}>Home</Link>
      {user ? (
        <>
          <span style={{ marginRight: '10px' }}>Hello, {user.username}</span>
          <button onClick={handleLogout}>Logout</button>
        </>
      ) : (
        <>
          <Link to="/login" style={{ marginRight: '10px' }}>Login</Link>
          <Link to="/register">Register</Link>
        </>
      )}
    </nav>
  );
};

export default Navbar;
