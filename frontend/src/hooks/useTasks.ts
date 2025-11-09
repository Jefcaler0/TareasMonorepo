import { useCallback, useEffect, useMemo, useState } from 'react';
import { taskApi } from '../services/api';
import type { CreateTaskPayload, Task, TaskStatus, UpdateTaskPayload } from '../types/task';

interface UseTasksState {
  tasks: Task[];
  isLoading: boolean;
  error: string | null;
  statusCounts: Record<TaskStatus, number>;
  createTask: (payload: CreateTaskPayload) => Promise<void>;
  updateTask: (id: number, payload: UpdateTaskPayload) => Promise<void>;
  deleteTask: (id: number) => Promise<void>;
  refresh: () => Promise<void>;
}

const initialStatusCounts: Record<TaskStatus, number> = {
  Pending: 0,
  InProgress: 0,
  Completed: 0
};

export const useTasks = (): UseTasksState => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const loadTasks = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await taskApi.list();
      setTasks(data);
    } catch (err) {
      setError('No se pudieron cargar las tareas. Intenta nuevamente.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadTasks();
  }, [loadTasks]);

  const createTask = useCallback(async (payload: CreateTaskPayload) => {
    try {
      setError(null);
      const created = await taskApi.create(payload);
      setTasks((prev) => [created, ...prev]);
    } catch {
      setError('No se pudo crear la tarea.');
    }
  }, []);

  const updateTask = useCallback(async (id: number, payload: UpdateTaskPayload) => {
    try {
      setError(null);
      const updated = await taskApi.update(id, payload);
      setTasks((prev) => prev.map((task) => (task.id === id ? updated : task)));
    } catch {
      setError('No se pudo actualizar la tarea.');
    }
  }, []);

  const deleteTask = useCallback(async (id: number) => {
    try {
      setError(null);
      await taskApi.remove(id);
      setTasks((prev) => prev.filter((task) => task.id !== id));
    } catch {
      setError('No se pudo eliminar la tarea.');
    }
  }, []);

  const statusCounts = useMemo(() => {
    return tasks.reduce<Record<TaskStatus, number>>((acc, task) => {
      acc[task.status] += 1;
      return acc;
    }, { ...initialStatusCounts });
  }, [tasks]);

  return {
    tasks,
    isLoading,
    error,
    statusCounts,
    createTask,
    updateTask,
    deleteTask,
    refresh: loadTasks
  };
};

