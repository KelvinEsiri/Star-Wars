using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Star_Wars.Repositories.Implementations;
using Star_Wars.Data;
using Star_Wars.Models;

namespace Star_Wars.Tests.UnitTests.Repositories;

public class StarshipRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly StarshipRepository _repository;

    public StarshipRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        _repository = new StarshipRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStarships()
    {
        // Arrange
        var starships = new List<Starship>
        {
            new() { Name = "X-wing", Model = "T-65 X-wing" },
            new() { Name = "TIE Fighter", Model = "Twin Ion Engine" }
        };
        _context.Starships.AddRange(starships);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Name == "X-wing");
        result.Should().Contain(s => s.Name == "TIE Fighter");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsStarship()
    {
        // Arrange
        var starship = new Starship { Name = "Millennium Falcon", Model = "YT-1300" };
        _context.Starships.Add(starship);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(starship.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Millennium Falcon");
        result.Id.Should().Be(starship.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsStarshipToDatabase()
    {
        // Arrange
        var starship = new Starship 
        { 
            Name = "Y-wing", 
            Model = "BTL Y-wing",
            Manufacturer = "Koensayr Manufacturing"
        };

        // Act
        var result = await _repository.AddAsync(starship);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("Y-wing");
        
        // Verify it's in the database
        var savedStarship = await _context.Starships.FindAsync(result.Id);
        savedStarship.Should().NotBeNull();
        savedStarship!.Name.Should().Be("Y-wing");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingStarship()
    {
        // Arrange
        var starship = new Starship { Name = "A-wing", Model = "RZ-1 A-wing" };
        _context.Starships.Add(starship);
        await _context.SaveChangesAsync();

        // Modify the starship
        starship.Name = "Modified A-wing";
        starship.Model = "RZ-1 Modified A-wing";

        // Act
        var result = await _repository.UpdateAsync(starship);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Modified A-wing");
        result.Model.Should().Be("RZ-1 Modified A-wing");
        
        // Verify it's updated in database
        var updatedStarship = await _context.Starships.FindAsync(starship.Id);
        updatedStarship!.Name.Should().Be("Modified A-wing");
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_RemovesStarshipAndReturnsTrue()
    {
        // Arrange
        var starship = new Starship { Name = "B-wing", Model = "A/SF-01 B-wing" };
        _context.Starships.Add(starship);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(starship.Id);

        // Assert
        result.Should().BeTrue();
        
        // Verify it's removed from database
        var deletedStarship = await _context.Starships.FindAsync(starship.Id);
        deletedStarship.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_WithMatchingTerm_ReturnsMatchingStarships()
    {
        // Arrange
        var starships = new List<Starship>
        {
            new() { Name = "X-wing", Model = "T-65 X-wing", Manufacturer = "Incom" },
            new() { Name = "Y-wing", Model = "BTL Y-wing", Manufacturer = "Koensayr" },
            new() { Name = "TIE Fighter", Model = "Twin Ion Engine", Manufacturer = "Sienar" }
        };
        _context.Starships.AddRange(starships);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("wing");

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Name == "X-wing");
        result.Should().Contain(s => s.Name == "Y-wing");
        result.Should().NotContain(s => s.Name == "TIE Fighter");
    }

    [Fact]
    public async Task SearchAsync_WithNonMatchingTerm_ReturnsEmptyList()
    {
        // Arrange
        var starship = new Starship { Name = "X-wing", Model = "T-65 X-wing" };
        _context.Starships.Add(starship);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
