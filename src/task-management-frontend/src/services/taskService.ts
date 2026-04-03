import { api } from './api';
import type { TaskItem, TaskCreateRequest, TaskUpdateRequest, User } from '../types';

export const taskService = {
  getAll: () => api.get<TaskItem[]>('/tasks'),
  getById: (id: string) => api.get<TaskItem>(`/tasks/${id}`),
  create: (data: TaskCreateRequest) => api.post<TaskItem>('/tasks', data),
  update: (id: string, data: TaskUpdateRequest) => api.put<TaskItem>(`/tasks/${id}`, data),
  delete: (id: string) => api.delete<void>(`/tasks/${id}`),
};

export const userService = {
  getAll: () => api.get<User[]>('/users'),
};
