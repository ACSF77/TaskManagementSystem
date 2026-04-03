import { useState, useEffect, useCallback } from 'react';
import { DragDropContext, Droppable, type DropResult } from '@hello-pangea/dnd';
import { useAuth } from '../context/AuthContext';
import { taskService, userService } from '../services/taskService';
import TaskCard from '../components/TaskCard';
import TaskForm from '../components/TaskForm';
import type { TaskItem, TaskCreateRequest, TaskUpdateRequest, User } from '../types';
import { TaskItemStatus, StatusLabels } from '../types';

const COLUMNS = [TaskItemStatus.Todo, TaskItemStatus.InProgress, TaskItemStatus.Done];

export default function TaskBoardPage() {
  const { user, logout } = useAuth();
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [editingTask, setEditingTask] = useState<TaskItem | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [loading, setLoading] = useState(true);

  const loadData = useCallback(async () => {
    try {
      const [tasksData, usersData] = await Promise.all([taskService.getAll(), userService.getAll()]);
      setTasks(tasksData);
      setUsers(usersData);
    } catch {
      // handled by api interceptor
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleDragEnd = async (result: DropResult) => {
    if (!result.destination) return;
    const newStatus = Number(result.destination.droppableId);
    const taskId = result.draggableId;
    const task = tasks.find(t => t.id === taskId);
    if (!task || task.status === newStatus) return;

    // optimistic update
    setTasks(prev => prev.map(t => t.id === taskId ? { ...t, status: newStatus, statusName: StatusLabels[newStatus] } : t));

    try {
      await taskService.update(taskId, {
        title: task.title,
        description: task.description,
        status: newStatus,
        dueDate: task.dueDate,
        assignedUserId: task.assignedUserId,
      });
    } catch {
      // revert on failure
      setTasks(prev => prev.map(t => t.id === taskId ? task : t));
    }
  };

  const handleSave = async (data: TaskCreateRequest | TaskUpdateRequest, id?: string) => {
    if (id) {
      await taskService.update(id, data as TaskUpdateRequest);
    } else {
      await taskService.create(data as TaskCreateRequest);
    }
    await loadData();
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this task?')) return;
    await taskService.delete(id);
    setTasks(prev => prev.filter(t => t.id !== id));
  };

  const openCreate = () => {
    setEditingTask(null);
    setShowForm(true);
  };

  const openEdit = (task: TaskItem) => {
    setEditingTask(task);
    setShowForm(true);
  };

  if (loading) return <div className="loading">Loading…</div>;

  return (
    <div className="board-page">
      <header className="board-header">
        <h1>Task Board</h1>
        <div className="header-actions">
          <span className="user-info">👤 {user?.username}</span>
          <button className="btn btn-primary" onClick={openCreate}>+ New Task</button>
          <button className="btn btn-secondary" onClick={logout}>Sign Out</button>
        </div>
      </header>

      <DragDropContext onDragEnd={handleDragEnd}>
        <div className="board">
          {COLUMNS.map(status => {
            const columnTasks = tasks.filter(t => t.status === status);
            return (
              <div key={status} className={`column column-${status}`}>
                <div className="column-header">
                  <h2>{StatusLabels[status]}</h2>
                  <span className="column-count">{columnTasks.length}</span>
                </div>
                <Droppable droppableId={String(status)}>
                  {(provided, snapshot) => (
                    <div
                      ref={provided.innerRef}
                      {...provided.droppableProps}
                      className={`column-body ${snapshot.isDraggingOver ? 'drag-over' : ''}`}
                    >
                      {columnTasks.map((task, index) => (
                        <TaskCard
                          key={task.id}
                          task={task}
                          index={index}
                          onEdit={openEdit}
                          onDelete={handleDelete}
                        />
                      ))}
                      {provided.placeholder}
                    </div>
                  )}
                </Droppable>
              </div>
            );
          })}
        </div>
      </DragDropContext>

      {showForm && (
        <TaskForm
          task={editingTask}
          users={users}
          onSave={handleSave}
          onClose={() => setShowForm(false)}
        />
      )}
    </div>
  );
}
