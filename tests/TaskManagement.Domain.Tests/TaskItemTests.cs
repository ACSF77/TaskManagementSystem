using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Tests;

public class TaskItemTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateTask()
    {
        var dueDate = DateTime.UtcNow.AddDays(7);

        var task = TaskItem.Create("Test Task", "A description", dueDate, _userId);

        task.Id.Should().NotBeEmpty();
        task.Title.Should().Be("Test Task");
        task.Description.Should().Be("A description");
        task.Status.Should().Be(TaskItemStatus.Todo);
        task.DueDate.Should().Be(dueDate);
        task.CreatedByUserId.Should().Be(_userId);
        task.AssignedUserId.Should().BeNull();
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        task.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithAssignedUser_ShouldSetAssignedUserId()
    {
        var assignee = Guid.NewGuid();
        var task = TaskItem.Create("Task", null, DateTime.UtcNow.AddDays(1), _userId, assignee);

        task.AssignedUserId.Should().Be(assignee);
    }

    [Fact]
    public void Create_ShouldTrimTitleAndDescription()
    {
        var task = TaskItem.Create("  My Task  ", "  Desc  ", DateTime.UtcNow.AddDays(1), _userId);

        task.Title.Should().Be("My Task");
        task.Description.Should().Be("Desc");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ShouldThrow(string? title)
    {
        var act = () => TaskItem.Create(title!, "desc", DateTime.UtcNow.AddDays(1), _userId);
        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Create_WithTitleExceeding200Chars_ShouldThrow()
    {
        var longTitle = new string('x', 201);
        var act = () => TaskItem.Create(longTitle, null, DateTime.UtcNow.AddDays(1), _userId);
        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Create_WithPastDueDate_ShouldThrow()
    {
        var act = () => TaskItem.Create("Task", null, DateTime.UtcNow.AddDays(-1), _userId);
        act.Should().Throw<ArgumentException>().WithParameterName("dueDate");
    }

    [Fact]
    public void Create_WithEmptyCreatorId_ShouldThrow()
    {
        var act = () => TaskItem.Create("Task", null, DateTime.UtcNow.AddDays(1), Guid.Empty);
        act.Should().Throw<ArgumentException>().WithParameterName("createdByUserId");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateAllFields()
    {
        var task = TaskItem.Create("Original", "Desc", DateTime.UtcNow.AddDays(5), _userId);
        var newDueDate = DateTime.UtcNow.AddDays(10);
        var assignee = Guid.NewGuid();

        task.Update("Updated Title", "New Desc", TaskItemStatus.InProgress, newDueDate, assignee);

        task.Title.Should().Be("Updated Title");
        task.Description.Should().Be("New Desc");
        task.Status.Should().Be(TaskItemStatus.InProgress);
        task.DueDate.Should().Be(newDueDate);
        task.AssignedUserId.Should().Be(assignee);
        task.UpdatedAt.Should().NotBeNull();
        task.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidTitle_ShouldThrow(string? title)
    {
        var task = TaskItem.Create("Task", null, DateTime.UtcNow.AddDays(1), _userId);
        var act = () => task.Update(title!, null, TaskItemStatus.Todo, DateTime.UtcNow.AddDays(1), null);
        act.Should().Throw<ArgumentException>().WithParameterName("title");
    }

    [Fact]
    public void Update_WithInvalidStatus_ShouldThrow()
    {
        var task = TaskItem.Create("Task", null, DateTime.UtcNow.AddDays(1), _userId);
        var act = () => task.Update("Task", null, (TaskItemStatus)99, DateTime.UtcNow.AddDays(1), null);
        act.Should().Throw<ArgumentException>().WithParameterName("status");
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_ShouldUpdateStatusAndTimestamp()
    {
        var task = TaskItem.Create("Task", null, DateTime.UtcNow.AddDays(1), _userId);

        task.UpdateStatus(TaskItemStatus.Done);

        task.Status.Should().Be(TaskItemStatus.Done);
        task.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStatus_WithInvalidStatus_ShouldThrow()
    {
        var task = TaskItem.Create("Task", null, DateTime.UtcNow.AddDays(1), _userId);
        var act = () => task.UpdateStatus((TaskItemStatus)42);
        act.Should().Throw<ArgumentException>().WithParameterName("newStatus");
    }

    [Fact]
    public void FromStorage_ShouldRestoreAllProperties()
    {
        var id = Guid.NewGuid();
        var assignee = Guid.NewGuid();
        var creator = Guid.NewGuid();
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var updated = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var dueDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var task = TaskItem.FromStorage(id, "Title", "Desc", TaskItemStatus.InProgress,
            dueDate, assignee, creator, created, updated);

        task.Id.Should().Be(id);
        task.Title.Should().Be("Title");
        task.Description.Should().Be("Desc");
        task.Status.Should().Be(TaskItemStatus.InProgress);
        task.DueDate.Should().Be(dueDate);
        task.AssignedUserId.Should().Be(assignee);
        task.CreatedByUserId.Should().Be(creator);
        task.CreatedAt.Should().Be(created);
        task.UpdatedAt.Should().Be(updated);
    }
}
