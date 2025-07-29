using Moq;
using FluentAssertions;
using Star_Wars.Services.Implementations;
using Star_Wars.Repositories.Interfaces;
using Star_Wars.Models;
using Star_Wars.DTOs;
using AutoMapper;

namespace Star_Wars.Tests.UnitTests.Services;

public class StarshipServiceTests
{
    private readonly Mock<IStarshipRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly StarshipService _service;

    public StarshipServiceTests()
    {
        _mockRepository = new Mock<IStarshipRepository>();
        _mockMapper = new Mock<IMapper>();
        _service = new StarshipService(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetAllStarshipsAsync_ReturnsAllStarships()
    {
        // Arrange
        var starships = new List<Starship>
        {
            new() { Id = 1, Name = "X-wing", Model = "T-65 X-wing" },
            new() { Id = 2, Name = "TIE Fighter", Model = "Twin Ion Engine" }
        };
        _mockRepository.Setup(r => r.GetAllAsync())
                      .ReturnsAsync(starships);

        // Act
        var result = await _service.GetAllStarshipsAsync();

        // Assert
        result.Should().BeEquivalentTo(starships);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetStarshipByIdAsync_WithValidId_ReturnsStarship()
    {
        // Arrange
        var starshipId = 1;
        var starship = new Starship { Id = starshipId, Name = "X-wing", Model = "T-65 X-wing" };
        _mockRepository.Setup(r => r.GetByIdAsync(starshipId))
                      .ReturnsAsync(starship);

        // Act
        var result = await _service.GetStarshipByIdAsync(starshipId);

        // Assert
        result.Should().BeEquivalentTo(starship);
        _mockRepository.Verify(r => r.GetByIdAsync(starshipId), Times.Once);
    }

    [Fact]
    public async Task GetStarshipByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var starshipId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(starshipId))
                      .ReturnsAsync((Starship?)null);

        // Act
        var result = await _service.GetStarshipByIdAsync(starshipId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(starshipId), Times.Once);
    }

    [Fact]
    public async Task CreateStarshipAsync_WithValidDto_ReturnsCreatedStarship()
    {
        // Arrange
        var createDto = new CreateStarshipDto
        {
            Name = "Y-wing",
            Model = "BTL Y-wing",
            Manufacturer = "Koensayr Manufacturing"
        };
        var starshipToCreate = new Starship
        {
            Name = createDto.Name,
            Model = createDto.Model,
            Manufacturer = createDto.Manufacturer
        };
        var createdStarship = new Starship
        {
            Id = 3,
            Name = createDto.Name,
            Model = createDto.Model,
            Manufacturer = createDto.Manufacturer
        };

        _mockMapper.Setup(m => m.Map<Starship>(createDto))
                  .Returns(starshipToCreate);
        _mockRepository.Setup(r => r.AddAsync(starshipToCreate))
                      .ReturnsAsync(createdStarship);

        // Act
        var result = await _service.CreateStarshipAsync(createDto);

        // Assert
        result.Should().BeEquivalentTo(createdStarship);
        _mockMapper.Verify(m => m.Map<Starship>(createDto), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(starshipToCreate), Times.Once);
    }

    [Fact]
    public async Task UpdateStarshipAsync_WithValidData_ReturnsUpdatedStarship()
    {
        // Arrange
        var starshipId = 1;
        var updateDto = new UpdateStarshipDto
        {
            Name = "Modified X-wing",
            Model = "T-65B X-wing"
        };
        var existingStarship = new Starship
        {
            Id = starshipId,
            Name = "X-wing",
            Model = "T-65 X-wing"
        };
        var updatedStarship = new Starship
        {
            Id = starshipId,
            Name = updateDto.Name,
            Model = updateDto.Model
        };

        _mockRepository.Setup(r => r.GetByIdAsync(starshipId))
                      .ReturnsAsync(existingStarship);
        _mockMapper.Setup(m => m.Map(updateDto, existingStarship));
        _mockRepository.Setup(r => r.UpdateAsync(existingStarship))
                      .ReturnsAsync(updatedStarship);

        // Act
        var result = await _service.UpdateStarshipAsync(starshipId, updateDto);

        // Assert
        result.Should().BeEquivalentTo(updatedStarship);
        _mockRepository.Verify(r => r.GetByIdAsync(starshipId), Times.Once);
        _mockMapper.Verify(m => m.Map(updateDto, existingStarship), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(existingStarship), Times.Once);
    }

    [Fact]
    public async Task UpdateStarshipAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var starshipId = 999;
        var updateDto = new UpdateStarshipDto { Name = "Test" };
        _mockRepository.Setup(r => r.GetByIdAsync(starshipId))
                      .ReturnsAsync((Starship?)null);

        // Act
        var result = await _service.UpdateStarshipAsync(starshipId, updateDto);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(starshipId), Times.Once);
        _mockMapper.Verify(m => m.Map(It.IsAny<UpdateStarshipDto>(), It.IsAny<Starship>()), Times.Never);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Starship>()), Times.Never);
    }

    [Fact]
    public async Task DeleteStarshipAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var starshipId = 1;
        var existingStarship = new Starship { Id = starshipId, Name = "X-wing" };
        _mockRepository.Setup(r => r.GetByIdAsync(starshipId))
                      .ReturnsAsync(existingStarship);
        _mockRepository.Setup(r => r.DeleteAsync(starshipId))
                      .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteStarshipAsync(starshipId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(r => r.GetByIdAsync(starshipId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(starshipId), Times.Once);
    }

    [Fact]
    public async Task DeleteStarshipAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        var starshipId = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(starshipId))
                      .ReturnsAsync((Starship?)null);

        // Act
        var result = await _service.DeleteStarshipAsync(starshipId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.GetByIdAsync(starshipId), Times.Once);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SearchStarshipsAsync_WithSearchTerm_ReturnsMatchingStarships()
    {
        // Arrange
        var searchTerm = "wing";
        var matchingStarships = new List<Starship>
        {
            new() { Id = 1, Name = "X-wing", Model = "T-65 X-wing" },
            new() { Id = 3, Name = "Y-wing", Model = "BTL Y-wing" }
        };
        _mockRepository.Setup(r => r.SearchAsync(searchTerm))
                      .ReturnsAsync(matchingStarships);

        // Act
        var result = await _service.SearchStarshipsAsync(searchTerm);

        // Assert
        result.Should().BeEquivalentTo(matchingStarships);
        _mockRepository.Verify(r => r.SearchAsync(searchTerm), Times.Once);
    }
}
