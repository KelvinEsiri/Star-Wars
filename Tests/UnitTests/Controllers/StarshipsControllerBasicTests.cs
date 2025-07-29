using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Star_Wars.Controllers;
using Star_Wars.Services.Interfaces;
using Star_Wars.DTOs;

namespace Star_Wars.Tests.UnitTests.Controllers;

public class StarshipsControllerBasicTests
{
    private readonly Mock<IStarshipService> _mockStarshipService;
    private readonly Mock<ILogger<StarshipsController>> _mockLogger;
    private readonly StarshipsController _controller;

    public StarshipsControllerBasicTests()
    {
        _mockStarshipService = new Mock<IStarshipService>();
        _mockLogger = new Mock<ILogger<StarshipsController>>();
        _controller = new StarshipsController(_mockStarshipService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Controller_ShouldBeCreatedSuccessfully()
    {
        // Act & Assert
        _controller.Should().NotBeNull();
        _controller.Should().BeAssignableTo<ControllerBase>();
    }

    [Fact]
    public async Task GetStarships_WithQuery_CallsService()
    {
        // Arrange
        var query = new StarshipQueryDto();
        var expectedResult = new PagedResult<StarshipDto>
        {
            Data = new List<StarshipDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _mockStarshipService.Setup(s => s.GetAllAsync(query))
                          .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.GetStarships(query);

        // Assert
        result.Should().NotBeNull();
        _mockStarshipService.Verify(s => s.GetAllAsync(query), Times.Once);
    }
}
