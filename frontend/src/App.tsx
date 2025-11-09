import { TaskForm } from './components/TaskForm';
import { TaskList } from './components/TaskList';
import { useTasks } from './hooks/useTasks';
import type { Task, TaskStatus } from './types/task';

const statusLabels: Record<TaskStatus, string> = {
  Pending: 'Pendientes',
  InProgress: 'En progreso',
  Completed: 'Completadas'
};

function App(): JSX.Element {
  const { tasks, isLoading, error, statusCounts, createTask, updateTask, deleteTask } = useTasks();

  const handleStatusChange = async (task: Task, status: TaskStatus): Promise<void> => {
    if (task.status === status) {
      return;
    }
    await updateTask(task.id, {
      title: task.title,
      description: task.description ?? undefined,
      status
    });
  };

  return (
    <div className="container">
      <header>
        <h1>Gestor de Tareas</h1>
        <p>Aplicación de ejemplo que consume la API de tareas utilizando Axios.</p>
      </header>

      <section className="metrics card">
        <h2>Resumen</h2>
        <div className="summary-grid">
          {Object.entries(statusCounts).map(([key, value]) => (
            <div key={key} className="summary-item">
              <span className="summary-title">{statusLabels[key as TaskStatus]}</span>
              <strong>{value}</strong>
            </div>
          ))}
        </div>
      </section>

      <section>
        <TaskForm
          onSubmit={async (payload) => {
            await createTask(payload);
          }}
        />
      </section>

      {error != null && <div className="card error">{error}</div>}

      <section>
        {isLoading ? <div className="card">Cargando tareas...</div> : <TaskList tasks={tasks} onStatusChange={handleStatusChange} onDelete={deleteTask} />}
      </section>
    </div>
  );
}

export default App;

