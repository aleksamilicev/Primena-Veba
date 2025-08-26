// src/api/models/User.js
class User {
  constructor({ username = "", email = "", password = "", profile_image_url = "" } = {}) {
    this.username = username;
    this.email = email;
    this.password = password;
    this.profile_image_url = profile_image_url;
  }
}

export default User;