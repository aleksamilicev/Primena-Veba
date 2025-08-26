// src/components/Profile/ViewProfile.js
import React, { useEffect, useState } from "react";
import { getProfile } from "../../api/services/userService";

const ViewProfile = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await getProfile();
        setUser(data);
      } catch (err) {
        setError("Greška pri učitavanju profila.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, []);

  if (loading) return <p>Loading profile...</p>;
  if (error) return <p>{error}</p>;

  return (
    <div>
      <h2>My Profile</h2>
      <img
        src={user.profileImageUrl || "https://via.placeholder.com/100"}
        alt="profile"
        style={{ width: "100px", height: "100px", borderRadius: "50%" }}
      />
      <p><strong>Username:</strong> {user.username}</p>
      <p><strong>Email:</strong> {user.email}</p>
    </div>
  );
};

export default ViewProfile;
