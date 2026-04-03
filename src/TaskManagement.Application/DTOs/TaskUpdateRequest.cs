using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class TaskUpdateRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public TaskItemStatus Status { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public Guid? AssignedUserId { get; set; }
}
