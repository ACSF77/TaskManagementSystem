using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskManagement.Application.DTOs;

namespace TaskManagement.WebAPI.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTasks_WithoutAuth_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_ThenLogin_ShouldReturnToken()
    {
        var registerRequest = new RegisterRequest
        {
            Username = $"integrationuser_{Guid.NewGuid():N}",
            Email = $"integration_{Guid.NewGuid():N}@test.com",
            Password = "IntTest123!"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginRequest = new LoginRequest { Username = registerRequest.Username, Password = "IntTest123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginResult!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginWithSeeded_ThenCrudTasks_ShouldWork()
    {
        // Login with seeded user
        var loginRequest = new LoginRequest { Username = "admin", Password = "Admin123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var token = loginResult!.Token;

        // GET all tasks (authenticated)
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/tasks");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getResponse = await _client.SendAsync(request);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tasks = await getResponse.Content.ReadFromJsonAsync<List<TaskResponse>>();
        tasks.Should().NotBeNull();
        tasks!.Count.Should().BeGreaterThan(0);

        // CREATE a task
        var createRequest = new TaskCreateRequest
        {
            Title = "Integration Test Task",
            Description = "Created by integration test",
            DueDate = DateTime.UtcNow.AddDays(30)
        };
        var createMsg = new HttpRequestMessage(HttpMethod.Post, "/api/tasks");
        createMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        createMsg.Content = JsonContent.Create(createRequest);
        var createResponse = await _client.SendAsync(createMsg);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        createdTask!.Title.Should().Be("Integration Test Task");

        // UPDATE the task
        var updateRequest = new TaskUpdateRequest
        {
            Title = "Updated Integration Task",
            Description = "Updated by integration test",
            Status = Domain.Enums.TaskItemStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(45)
        };
        var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/api/tasks/{createdTask.Id}");
        updateMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        updateMsg.Content = JsonContent.Create(updateRequest);
        var updateResponse = await _client.SendAsync(updateMsg);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // DELETE the task
        var deleteMsg = new HttpRequestMessage(HttpMethod.Delete, $"/api/tasks/{createdTask.Id}");
        deleteMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var deleteResponse = await _client.SendAsync(deleteMsg);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetUsers_WithAuth_ShouldReturnUsers()
    {
        var loginRequest = new LoginRequest { Username = "admin", Password = "Admin123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterOrEqualTo(2);
    }
}
