import { useState, useEffect, type FormEvent } from 'react';
import type { TaskItem, TaskCreateRequest, TaskUpdateRequest, User } from '../types';
import { TaskItemStatus } from '../types';

interface Props {
  task: TaskItem | null;
  users: User[];
  onSave: (data: TaskCreateRequest | TaskUpdateRequest, id?: string) => Promise<void>;
  onClose: () => void;
}

export default function TaskForm({ task, users, onSave, onClose }: Props) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [status, setStatus] = useState<number>(TaskItemStatus.Todo);
  const [dueDate, setDueDate] = useState('');
  const [assignedUserId, setAssignedUserId] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (task) {
      setTitle(task.title);
      setDescription(task.description || '');
      setStatus(task.status);
      setDueDate(task.dueDate.split('T')[0]);
      setAssignedUserId(task.assignedUserId || '');
    } else {
      const tomorrow = new Date();
      tomorrow.setDate(tomorrow.getDate() + 1);
      setDueDate(tomorrow.toISOString().split('T')[0]);
    }
  }, [task]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      if (task) {
        const data: TaskUpdateRequest = {
          title,
          description: description || null,
          status,
          dueDate: new Date(dueDate).toISOString(),
          assignedUserId: assignedUserId || null,
        };
        await onSave(data, task.id);
      } else {
        const data: TaskCreateRequest = {
          title,
          description: description || undefined,
          dueDate: new Date(dueDate).toISOString(),
          assignedUserId: assignedUserId || null,
        };
        await onSave(data);
      }
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save task');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" onClick={e => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{task ? 'Edit Task' : 'New Task'}</h2>
          <button className="btn-icon" onClick={onClose}>✕</button>
        </div>
        {error && <div className="error-message">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="title">Title</label>
            <input id="title" type="text" value={title} onChange={e => setTitle(e.target.value)} required maxLength={200} autoFocus />
          </div>
          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea id="description" value={description} onChange={e => setDescription(e.target.value)} rows={3} />
          </div>
          {task && (
            <div className="form-group">
              <label htmlFor="status">Status</label>
              <select id="status" value={status} onChange={e => setStatus(Number(e.target.value))}>
                <option value={0}>To Do</option>
                <option value={1}>In Progress</option>
                <option value={2}>Done</option>
              </select>
            </div>
          )}
          <div className="form-group">
            <label htmlFor="dueDate">Due Date</label>
            <input id="dueDate" type="date" value={dueDate} onChange={e => setDueDate(e.target.value)} required />
          </div>
          <div className="form-group">
            <label htmlFor="assignedUserId">Assigned To</label>
            <select id="assignedUserId" value={assignedUserId} onChange={e => setAssignedUserId(e.target.value)}>
              <option value="">Unassigned</option>
              {users.map(u => (
                <option key={u.id} value={u.id}>{u.username}</option>
              ))}
            </select>
          </div>
          <div className="modal-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
            <button type="submit" className="btn btn-primary" disabled={loading}>
              {loading ? 'Saving…' : task ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
