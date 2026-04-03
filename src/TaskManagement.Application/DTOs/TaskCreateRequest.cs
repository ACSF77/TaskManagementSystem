using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Application.DTOs;

public class TaskCreateRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public Guid? AssignedUserId { get; set; }
}
