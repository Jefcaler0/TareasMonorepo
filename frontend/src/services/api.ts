import axios from 'axios';
import type { CreateTaskPayload, Task, UpdateTaskPayload } from '../types/task';

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5062';

export const api = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json'
  }
});

export const taskApi = {
  async list(): Promise<Task[]> {
    const response = await api.get<Task[]>('/api/tasks');
    return response.data;
  },

  async create(payload: CreateTaskPayload): Promise<Task> {
    const response = await api.post<Task>('/api/tasks', payload);
    return response.data;
  },

  async update(id: number, payload: UpdateTaskPayload): Promise<Task> {
    const response = await api.put<Task>(`/api/tasks/${id}`, payload);
    return response.data;
  },

  async remove(id: number): Promise<void> {
    await api.delete(`/api/tasks/${id}`);
  }
};

