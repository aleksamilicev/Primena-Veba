// src/components/Layout/ProfileDropdown.js
import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

const ProfileDropdown = () => {
  const [open, setOpen] = useState(false);
  const navigate = useNavigate();
  const user = JSON.parse(localStorage.getItem("user"));

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    navigate("/login");
  };

  return (
    <div style={{ position: "relative", display: "inline-block" }}>
      <img
        src={user?.profile_image_url || "https://via.placeholder.com/40"}
        alt="profile"
        style={{ width: "40px", height: "40px", borderRadius: "50%", cursor: "pointer" }}
        onClick={() => setOpen(!open)}
      />
      {open && (
        <div
          style={{
            position: "absolute",
            right: 0,
            backgroundColor: "#fff",
            border: "1px solid #ccc",
            borderRadius: "5px",
            boxShadow: "0px 2px 5px rgba(0,0,0,0.2)",
            marginTop: "5px",
            minWidth: "150px",
            zIndex: 1000,
          }}
        >
          <button onClick={() => navigate("/profile")} style={{ display: "block", width: "100%", padding: "10px", border: "none", background: "none", textAlign: "left" }}>
            View Profile
          </button>
          <button onClick={() => navigate("/profile/edit")} style={{ display: "block", width: "100%", padding: "10px", border: "none", background: "none", textAlign: "left" }}>
            Edit Profile
          </button>
          <button onClick={handleLogout} style={{ display: "block", width: "100%", padding: "10px", border: "none", background: "none", textAlign: "left" }}>
            Logout
          </button>
        </div>
      )}
    </div>
  );
};

export default ProfileDropdown;
