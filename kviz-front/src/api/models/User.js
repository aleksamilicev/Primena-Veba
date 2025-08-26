// src/api/models/User.js
class User {
  constructor({ username = "", email = "", password = "", profileImageUrl = "" } = {}) {
    this.username = username;
    this.email = email;
    this.password = password;
    this.profileImageUrl = profileImageUrl;
  }
}

export default User;