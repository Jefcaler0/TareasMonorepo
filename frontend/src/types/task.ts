export type TaskStatus = 'Pending' | 'InProgress' | 'Completed';

export interface Task {
  id: number;
  title: string;
  description?: string | null;
  status: TaskStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskPayload {
  title: string;
  description?: string;
  status: TaskStatus;
}

export interface UpdateTaskPayload extends CreateTaskPayload {}

