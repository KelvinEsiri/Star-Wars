using FluentAssertions;
using Star_Wars.Models;

namespace Star_Wars.Tests.UnitTests;

public class BasicModelTests
{
    [Fact]
    public void Starship_CanBeCreated()
    {
        // Arrange & Act
        var starship = new Starship
        {
            Name = "X-wing",
            Model = "T-65 X-wing",
            Manufacturer = "Incom Corporation"
        };

        // Assert
        starship.Should().NotBeNull();
        starship.Name.Should().Be("X-wing");
        starship.Model.Should().Be("T-65 X-wing");
        starship.Manufacturer.Should().Be("Incom Corporation");
    }

    [Fact]
    public void Starship_Properties_CanBeSet()
    {
        // Arrange
        var starship = new Starship();

        // Act
        starship.Id = 1;
        starship.Name = "TIE Fighter";
        starship.Model = "Twin Ion Engine";

        // Assert
        starship.Id.Should().Be(1);
        starship.Name.Should().Be("TIE Fighter");
        starship.Model.Should().Be("Twin Ion Engine");
    }
}
