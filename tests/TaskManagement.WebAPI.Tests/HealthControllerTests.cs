using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.WebAPI.Controllers;

namespace TaskManagement.WebAPI.Tests;

public class HealthControllerTests
{
    [Fact]
    public void Get_ShouldReturnOk()
    {
        var sut = new HealthController();

        var result = sut.Get();

        result.Should().BeOfType<OkObjectResult>();
    }
}
