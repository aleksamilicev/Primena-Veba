// src/components/Profile/EditProfile.js
import React, { useEffect, useState } from "react";
import { getProfile, updateProfile } from "../../api/services/userService";
import "../../styles/Profile.css";

const EditProfile = () => {
  const [user, setUser] = useState({ username: "", email: "", profile_image_url: "" });
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState("");

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const data = await getProfile();
        setUser(data);
      } catch (err) {
        setMessage("Greška pri učitavanju profila.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, []);

  const handleChange = (e) => {
    setUser({ ...user, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await updateProfile(user);
      setMessage("Profil uspešno ažuriran!");
    } catch (err) {
      setMessage("Greška pri ažuriranju profila.");
      console.error(err);
    }
  };

  if (loading) return <p>Loading profile...</p>;

  return (
    <div className="profile-card">
      <h2>Edit Profile</h2>
      <form className="profile-form" onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Username</label>
          <input type="text" name="username" value={user.username} onChange={handleChange} disabled />
        </div>

        <div className="form-group">
          <label>Email</label>
          <input type="email" name="email" value={user.email} onChange={handleChange} />
        </div>

        <div className="form-group">
          <label>Profile Image URL</label>
          <input type="text" name="profile_image_url" value={user.profile_image_url} onChange={handleChange} />
        </div>

        <button type="submit" className="save-btn">Save Changes</button>
      </form>
      {message && <p className="profile-message">{message}</p>}
    </div>
  );
};

export default EditProfile;
