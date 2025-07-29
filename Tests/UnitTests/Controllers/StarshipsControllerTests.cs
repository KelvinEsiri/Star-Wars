using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Star_Wars.Controllers;
using Star_Wars.Services.Interfaces;
using Star_Wars.Models;
using Star_Wars.DTOs;

namespace Star_Wars.Tests.UnitTests.Controllers;

public class StarshipsControllerTests
{
    private readonly Mock<IStarshipService> _mockStarshipService;
    private readonly StarshipsController _controller;

    public StarshipsControllerTests()
    {
        _mockStarshipService = new Mock<IStarshipService>();
        _controller = new StarshipsController(_mockStarshipService.Object);
    }

    [Fact]
    public async Task GetStarships_ReturnsOkResult_WithStarshipsList()
    {
        // Arrange
        var starships = new List<Starship>
        {
            new() { Id = 1, Name = "X-wing", Model = "T-65 X-wing" },
            new() { Id = 2, Name = "TIE Fighter", Model = "Twin Ion Engine" }
        };
        _mockStarshipService.Setup(s => s.GetAllStarshipsAsync())
                           .ReturnsAsync(starships);

        // Act
        var result = await _controller.GetStarships();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(starships);
        _mockStarshipService.Verify(s => s.GetAllStarshipsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetStarship_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var starshipId = 1;
        var starship = new Starship { Id = starshipId, Name = "X-wing", Model = "T-65 X-wing" };
        _mockStarshipService.Setup(s => s.GetStarshipByIdAsync(starshipId))
                           .ReturnsAsync(starship);

        // Act
        var result = await _controller.GetStarship(starshipId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(starship);
    }

    [Fact]
    public async Task GetStarship_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var starshipId = 999;
        _mockStarshipService.Setup(s => s.GetStarshipByIdAsync(starshipId))
                           .ReturnsAsync((Starship?)null);

        // Act
        var result = await _controller.GetStarship(starshipId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateStarship_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateStarshipDto
        {
            Name = "Y-wing",
            Model = "BTL Y-wing",
            Manufacturer = "Koensayr Manufacturing"
        };
        var createdStarship = new Starship 
        { 
            Id = 3, 
            Name = createDto.Name, 
            Model = createDto.Model,
            Manufacturer = createDto.Manufacturer
        };
        _mockStarshipService.Setup(s => s.CreateStarshipAsync(It.IsAny<CreateStarshipDto>()))
                           .ReturnsAsync(createdStarship);

        // Act
        var result = await _controller.CreateStarship(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdStarship);
        createdResult.ActionName.Should().Be(nameof(StarshipsController.GetStarship));
        createdResult.RouteValues!["id"].Should().Be(createdStarship.Id);
    }

    [Fact]
    public async Task UpdateStarship_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var starshipId = 1;
        var updateDto = new UpdateStarshipDto
        {
            Name = "Modified X-wing",
            Model = "T-65B X-wing"
        };
        var updatedStarship = new Starship
        {
            Id = starshipId,
            Name = updateDto.Name,
            Model = updateDto.Model
        };
        _mockStarshipService.Setup(s => s.UpdateStarshipAsync(starshipId, updateDto))
                           .ReturnsAsync(updatedStarship);

        // Act
        var result = await _controller.UpdateStarship(starshipId, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedStarship);
    }

    [Fact]
    public async Task UpdateStarship_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var starshipId = 999;
        var updateDto = new UpdateStarshipDto { Name = "Test" };
        _mockStarshipService.Setup(s => s.UpdateStarshipAsync(starshipId, updateDto))
                           .ReturnsAsync((Starship?)null);

        // Act
        var result = await _controller.UpdateStarship(starshipId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteStarship_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var starshipId = 1;
        _mockStarshipService.Setup(s => s.DeleteStarshipAsync(starshipId))
                           .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteStarship(starshipId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteStarship_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var starshipId = 999;
        _mockStarshipService.Setup(s => s.DeleteStarshipAsync(starshipId))
                           .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteStarship(starshipId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
