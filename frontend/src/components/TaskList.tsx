import { Fragment } from 'react';
import type { Task, TaskStatus } from '../types/task';

interface TaskListProps {
  tasks: Task[];
  onStatusChange: (task: Task, status: TaskStatus) => Promise<void> | void;
  onDelete: (taskId: number) => Promise<void> | void;
}

const statusLabels: Record<TaskStatus, string> = {
  Pending: 'Pendiente',
  InProgress: 'En progreso',
  Completed: 'Completada'
};

const statusOrder: TaskStatus[] = ['Pending', 'InProgress', 'Completed'];

export const TaskList: React.FC<TaskListProps> = ({ tasks, onStatusChange, onDelete }) => {
  if (tasks.length === 0) {
    return (
      <div className="card empty-state">
        <p>No hay tareas registradas.</p>
      </div>
    );
  }

  return (
    <div className="task-list">
      {tasks.map((task) => (
        <div key={task.id} className="card task-item">
          <header>
            <h3>{task.title}</h3>
            <span className={`status ${task.status.toLowerCase()}`}>{statusLabels[task.status]}</span>
          </header>
          {task.description != null && task.description.length > 0 && <p>{task.description}</p>}
          <div className="task-actions">
            <label>
              Estado
              <select
                value={task.status}
                onChange={async (event) => {
                  await onStatusChange(task, event.target.value as TaskStatus);
                }}
              >
                {statusOrder.map((option) => (
                  <option key={option} value={option}>
                    {statusLabels[option]}
                  </option>
                ))}
              </select>
            </label>
            <button
              type="button"
              className="danger"
              onClick={async () => {
                await onDelete(task.id);
              }}
            >
              Eliminar
            </button>
          </div>
          <footer>
            <small>
              Creada: {new Date(task.createdAt).toLocaleString()} | Actualizada: {new Date(task.updatedAt).toLocaleString()}
            </small>
          </footer>
        </div>
      ))}
    </div>
  );
};

