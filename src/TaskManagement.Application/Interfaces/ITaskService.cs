using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskResponse>> GetAllAsync();
    Task<TaskResponse> CreateAsync(TaskCreateRequest request, Guid createdByUserId);
    Task<TaskResponse> UpdateAsync(Guid id, TaskUpdateRequest request);
    Task DeleteAsync(Guid id);
}
