using FluentAssertions;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly TaskService _sut;
    private readonly User _testUser;
    private readonly User _testAssignee;

    public TaskServiceTests()
    {
        _taskRepoMock = new Mock<ITaskRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _sut = new TaskService(_taskRepoMock.Object, _userRepoMock.Object);

        _testUser = User.Create("creator", "creator@test.com", "hash");
        _testAssignee = User.Create("assignee", "assignee@test.com", "hash");

        _userRepoMock.Setup(r => r.GetByIdAsync(_testUser.Id)).ReturnsAsync(_testUser);
        _userRepoMock.Setup(r => r.GetByIdAsync(_testAssignee.Id)).ReturnsAsync(_testAssignee);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingTask_ShouldReturnTask()
    {
        var task = TaskItem.Create("Test", "Desc", DateTime.UtcNow.AddDays(5), _testUser.Id);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        var result = await _sut.GetByIdAsync(task.Id);

        result.Title.Should().Be("Test");
        result.Description.Should().Be("Desc");
        result.Status.Should().Be(TaskItemStatus.Todo);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingTask_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((TaskItem?)null);

        var act = () => _sut.GetByIdAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTasks()
    {
        var tasks = new List<TaskItem>
        {
            TaskItem.Create("Task 1", null, DateTime.UtcNow.AddDays(1), _testUser.Id),
            TaskItem.Create("Task 2", null, DateTime.UtcNow.AddDays(2), _testUser.Id)
        };
        _taskRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(tasks);

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateAndReturnTask()
    {
        var request = new TaskCreateRequest
        {
            Title = "New Task",
            Description = "A task",
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = _testAssignee.Id
        };

        var result = await _sut.CreateAsync(request, _testUser.Id);

        result.Title.Should().Be("New Task");
        result.Description.Should().Be("A task");
        result.Status.Should().Be(TaskItemStatus.Todo);
        result.AssignedUserId.Should().Be(_testAssignee.Id);
        result.AssignedUsername.Should().Be("assignee");
        _taskRepoMock.Verify(r => r.CreateAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAssignee_ShouldThrowNotFound()
    {
        var badId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(badId)).ReturnsAsync((User?)null);

        var request = new TaskCreateRequest
        {
            Title = "Task",
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = badId
        };

        var act = () => _sut.CreateAsync(request, _testUser.Id);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Assigned user*");
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_ShouldUpdateAndReturnTask()
    {
        var task = TaskItem.Create("Old", "Desc", DateTime.UtcNow.AddDays(5), _testUser.Id);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        var request = new TaskUpdateRequest
        {
            Title = "Updated",
            Description = "New desc",
            Status = TaskItemStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(10),
            AssignedUserId = _testAssignee.Id
        };

        var result = await _sut.UpdateAsync(task.Id, request);

        result.Title.Should().Be("Updated");
        result.Status.Should().Be(TaskItemStatus.InProgress);
        result.AssignedUserId.Should().Be(_testAssignee.Id);
        _taskRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingTask_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((TaskItem?)null);

        var request = new TaskUpdateRequest
        {
            Title = "X",
            Status = TaskItemStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(1)
        };

        var act = () => _sut.UpdateAsync(id, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingTask_ShouldDelete()
    {
        var task = TaskItem.Create("Delete Me", null, DateTime.UtcNow.AddDays(1), _testUser.Id);
        _taskRepoMock.Setup(r => r.GetByIdAsync(task.Id)).ReturnsAsync(task);

        await _sut.DeleteAsync(task.Id);

        _taskRepoMock.Verify(r => r.DeleteAsync(task.Id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingTask_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _taskRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((TaskItem?)null);

        var act = () => _sut.DeleteAsync(id);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
