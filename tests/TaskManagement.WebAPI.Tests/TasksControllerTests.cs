using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Enums;
using TaskManagement.WebAPI.Controllers;

namespace TaskManagement.WebAPI.Tests;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly TasksController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _sut = new TasksController(_taskServiceMock.Object);

        // Set up authenticated user
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithTasks()
    {
        var tasks = new List<TaskResponse>
        {
            new() { Id = Guid.NewGuid(), Title = "Task 1" },
            new() { Id = Guid.NewGuid(), Title = "Task 2" }
        };
        _taskServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(tasks);

        var result = await _sut.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<TaskResponse>>().Subject;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WithExistingTask_ShouldReturnOk()
    {
        var id = Guid.NewGuid();
        var task = new TaskResponse { Id = id, Title = "Test" };
        _taskServiceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(task);

        var result = await _sut.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        ((TaskResponse)okResult.Value!).Id.Should().Be(id);
    }

    [Fact]
    public async Task Create_WithValidRequest_ShouldReturnCreated()
    {
        var request = new TaskCreateRequest { Title = "New Task", DueDate = DateTime.UtcNow.AddDays(5) };
        var response = new TaskResponse { Id = Guid.NewGuid(), Title = "New Task" };
        _taskServiceMock.Setup(s => s.CreateAsync(request, _userId)).ReturnsAsync(response);

        var result = await _sut.Create(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Update_WithValidRequest_ShouldReturnOk()
    {
        var id = Guid.NewGuid();
        var request = new TaskUpdateRequest { Title = "Updated", Status = TaskItemStatus.InProgress, DueDate = DateTime.UtcNow.AddDays(5) };
        var response = new TaskResponse { Id = id, Title = "Updated", Status = TaskItemStatus.InProgress };
        _taskServiceMock.Setup(s => s.UpdateAsync(id, request)).ReturnsAsync(response);

        var result = await _sut.Update(id, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        ((TaskResponse)okResult.Value!).Title.Should().Be("Updated");
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        var id = Guid.NewGuid();

        var result = await _sut.Delete(id);

        result.Should().BeOfType<NoContentResult>();
        _taskServiceMock.Verify(s => s.DeleteAsync(id), Times.Once);
    }
}
