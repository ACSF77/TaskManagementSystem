export interface User {
  id: string;
  username: string;
  email: string;
  createdAt: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  email: string;
  userId: string;
}

export interface TaskItem {
  id: string;
  title: string;
  description: string | null;
  status: number;
  statusName: string;
  dueDate: string;
  assignedUserId: string | null;
  assignedUsername: string | null;
  createdByUserId: string;
  createdByUsername: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface TaskCreateRequest {
  title: string;
  description?: string;
  dueDate: string;
  assignedUserId?: string | null;
}

export interface TaskUpdateRequest {
  title: string;
  description?: string | null;
  status: number;
  dueDate: string;
  assignedUserId?: string | null;
}

export const TaskItemStatus = {
  Todo: 0,
  InProgress: 1,
  Done: 2,
} as const;

export const StatusLabels: Record<number, string> = {
  0: 'To Do',
  1: 'In Progress',
  2: 'Done',
};
