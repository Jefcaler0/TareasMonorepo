import { useState } from 'react';
import type { CreateTaskPayload, TaskStatus } from '../types/task';

const statusOptions: Array<{ value: TaskStatus; label: string }> = [
  { value: 'Pending', label: 'Pendiente' },
  { value: 'InProgress', label: 'En progreso' },
  { value: 'Completed', label: 'Completada' }
];

interface TaskFormProps {
  onSubmit: (payload: CreateTaskPayload) => Promise<void> | void;
}

export const TaskForm: React.FC<TaskFormProps> = ({ onSubmit }) => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [status, setStatus] = useState<TaskStatus>('Pending');

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>): Promise<void> => {
    event.preventDefault();
    if (title.trim().length === 0) {
      return;
    }

    await onSubmit({
      title: title.trim(),
      description: description.trim() === '' ? undefined : description.trim(),
      status
    });

    setTitle('');
    setDescription('');
    setStatus('Pending');
  };

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>Crear nueva tarea</h2>
      <label>
        Título
        <input
          type="text"
          value={title}
          maxLength={100}
          onChange={(event) => {
            setTitle(event.target.value);
          }}
          placeholder="Agregar título"
          required
        />
      </label>

      <label>
        Descripción
        <textarea
          value={description}
          maxLength={500}
          onChange={(event) => {
            setDescription(event.target.value);
          }}
          placeholder="Opcional"
          rows={3}
        />
      </label>

      <label>
        Estado
        <select
          value={status}
          onChange={(event) => {
            setStatus(event.target.value as TaskStatus);
          }}
        >
          {statusOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>

      <button type="submit">Guardar tarea</button>
    </form>
  );
};

