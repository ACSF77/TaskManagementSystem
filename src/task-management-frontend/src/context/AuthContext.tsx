import { createContext, useContext, useState, type ReactNode } from 'react';
import { authService } from '../services/authService';
import type { LoginRequest, RegisterRequest } from '../types';

interface AuthUser {
  userId: string;
  username: string;
  email: string;
}

interface AuthContextType {
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => {
    const stored = localStorage.getItem('user');
    const token = localStorage.getItem('token');
    if (stored && token) {
      return JSON.parse(stored);
    }
    return null;
  });

  const login = async (data: LoginRequest) => {
    const response = await authService.login(data);
    const authUser: AuthUser = {
      userId: response.userId,
      username: response.username,
      email: response.email,
    };
    localStorage.setItem('token', response.token);
    localStorage.setItem('user', JSON.stringify(authUser));
    setUser(authUser);
  };

  const register = async (data: RegisterRequest) => {
    await authService.register(data);
  };

  const logout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
