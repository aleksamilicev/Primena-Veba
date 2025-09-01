import { createContext, useContext, useState, useEffect } from "react";
import { jwtDecode } from "jwt-decode";

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);

  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
  }, []);

  const login = (token) => {
    const decoded = jwtDecode(token);

    // pokušaj uzeti username iz "unique_name", pa "name", pa "sub"
    const username = decoded.unique_name || decoded.name || decoded.sub;

    // ClaimTypes.Name se mapira na "name" u tokenu
    const userData = {
      username: username, 
      email: decoded.email,
      isAdmin: decoded.IsAdmin === "1" || decoded.IsAdmin === 1,
      token
    };

    setUser(userData);
    localStorage.setItem("user", JSON.stringify(userData));
    localStorage.setItem("token", token);
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem("user");
    localStorage.removeItem("token");
  };

  // Za istek tokena
  useEffect(() => {
    if (user?.token) {
      const decoded = jwtDecode(user.token);
      const exp = decoded.exp; // vreme u sekundama
      const now = Date.now() / 1000;

      if (exp < now) {
        console.log("Token je istekao, logout");
        logout();
      } else {
        const timeout = (exp - now) * 1000;
        const timer = setTimeout(() => logout(), timeout);
        return () => clearTimeout(timer);
      }
    }
  }, [user]);




  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
