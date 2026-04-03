using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime DueDate { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private TaskItem() { }

    public static TaskItem Create(string title, string? description, DateTime dueDate, Guid createdByUserId, Guid? assignedUserId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (title.Length > 200)
            throw new ArgumentException("Title must not exceed 200 characters.", nameof(title));
        if (dueDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Due date cannot be in the past.", nameof(dueDate));
        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Creator user ID is required.", nameof(createdByUserId));

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            Status = TaskItemStatus.Todo,
            DueDate = dueDate,
            AssignedUserId = assignedUserId,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? description, TaskItemStatus status, DateTime dueDate, Guid? assignedUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (title.Length > 200)
            throw new ArgumentException("Title must not exceed 200 characters.", nameof(title));
        if (!Enum.IsDefined(typeof(TaskItemStatus), status))
            throw new ArgumentException("Invalid status value.", nameof(status));

        Title = title.Trim();
        Description = description?.Trim();
        Status = status;
        DueDate = dueDate;
        AssignedUserId = assignedUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(TaskItemStatus newStatus)
    {
        if (!Enum.IsDefined(typeof(TaskItemStatus), newStatus))
            throw new ArgumentException("Invalid status value.", nameof(newStatus));

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public static TaskItem FromStorage(Guid id, string title, string? description, TaskItemStatus status,
        DateTime dueDate, Guid? assignedUserId, Guid createdByUserId, DateTime createdAt, DateTime? updatedAt)
    {
        return new TaskItem
        {
            Id = id,
            Title = title,
            Description = description,
            Status = status,
            DueDate = dueDate,
            AssignedUserId = assignedUserId,
            CreatedByUserId = createdByUserId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
