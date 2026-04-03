using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new NotFoundException($"Task with ID '{id}' was not found.");

        return await MapToResponseAsync(task);
    }

    public async Task<IEnumerable<TaskResponse>> GetAllAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        var responses = new List<TaskResponse>();

        foreach (var task in tasks)
        {
            responses.Add(await MapToResponseAsync(task));
        }

        return responses;
    }

    public async Task<TaskResponse> CreateAsync(TaskCreateRequest request, Guid createdByUserId)
    {
        if (request.AssignedUserId.HasValue)
        {
            var assignee = await _userRepository.GetByIdAsync(request.AssignedUserId.Value);
            if (assignee == null)
                throw new NotFoundException($"Assigned user with ID '{request.AssignedUserId}' was not found.");
        }

        var task = TaskItem.Create(
            request.Title,
            request.Description,
            request.DueDate,
            createdByUserId,
            request.AssignedUserId
        );

        await _taskRepository.CreateAsync(task);

        return await MapToResponseAsync(task);
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, TaskUpdateRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new NotFoundException($"Task with ID '{id}' was not found.");

        if (request.AssignedUserId.HasValue)
        {
            var assignee = await _userRepository.GetByIdAsync(request.AssignedUserId.Value);
            if (assignee == null)
                throw new NotFoundException($"Assigned user with ID '{request.AssignedUserId}' was not found.");
        }

        task.Update(request.Title, request.Description, request.Status, request.DueDate, request.AssignedUserId);

        await _taskRepository.UpdateAsync(task);

        return await MapToResponseAsync(task);
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
            throw new NotFoundException($"Task with ID '{id}' was not found.");

        await _taskRepository.DeleteAsync(id);
    }

    private async Task<TaskResponse> MapToResponseAsync(TaskItem task)
    {
        string? assignedUsername = null;
        if (task.AssignedUserId.HasValue)
        {
            var assignee = await _userRepository.GetByIdAsync(task.AssignedUserId.Value);
            assignedUsername = assignee?.Username;
        }

        var creator = await _userRepository.GetByIdAsync(task.CreatedByUserId);

        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            StatusName = task.Status.ToString(),
            DueDate = task.DueDate,
            AssignedUserId = task.AssignedUserId,
            AssignedUsername = assignedUsername,
            CreatedByUserId = task.CreatedByUserId,
            CreatedByUsername = creator?.Username ?? "Unknown",
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}
