import type { TaskItem } from '../types';
import { Draggable } from '@hello-pangea/dnd';

interface Props {
  task: TaskItem;
  index: number;
  onEdit: (task: TaskItem) => void;
  onDelete: (id: string) => void;
}

export default function TaskCard({ task, index, onEdit, onDelete }: Props) {
  const isOverdue = new Date(task.dueDate) < new Date() && task.status !== 2;

  return (
    <Draggable draggableId={task.id} index={index}>
      {(provided, snapshot) => (
        <div
          ref={provided.innerRef}
          {...provided.draggableProps}
          {...provided.dragHandleProps}
          className={`task-card ${snapshot.isDragging ? 'dragging' : ''} ${isOverdue ? 'overdue' : ''}`}
        >
          <div className="task-card-header">
            <h4 className="task-title">{task.title}</h4>
            <div className="task-actions">
              <button className="btn-icon" onClick={() => onEdit(task)} title="Edit">✏️</button>
              <button className="btn-icon" onClick={() => onDelete(task.id)} title="Delete">🗑️</button>
            </div>
          </div>
          {task.description && <p className="task-description">{task.description}</p>}
          <div className="task-meta">
            <span className={`task-due ${isOverdue ? 'overdue-text' : ''}`}>
              📅 {new Date(task.dueDate).toLocaleDateString()}
            </span>
            {task.assignedUsername && (
              <span className="task-assigned">👤 {task.assignedUsername}</span>
            )}
          </div>
          <div className="task-footer">
            <span className="task-creator">Created by {task.createdByUsername}</span>
          </div>
        </div>
      )}
    </Draggable>
  );
}
