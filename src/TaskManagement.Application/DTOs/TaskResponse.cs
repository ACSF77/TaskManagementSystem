using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUsername { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
