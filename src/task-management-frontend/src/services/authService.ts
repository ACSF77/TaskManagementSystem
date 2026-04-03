import { api } from './api';
import type { LoginRequest, LoginResponse, RegisterRequest, User } from '../types';

export const authService = {
  login: (data: LoginRequest) => api.post<LoginResponse>('/auth/login', data),
  register: (data: RegisterRequest) => api.post<User>('/auth/register', data),
};
