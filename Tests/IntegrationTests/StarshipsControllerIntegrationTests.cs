using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Star_Wars.Data;
using Star_Wars.Models;
using Star_Wars.DTOs;
using Newtonsoft.Json;

namespace Star_Wars.Tests.IntegrationTests;

public class StarshipsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public StarshipsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetStarships_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/starships");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString()
            .Should().Contain("application/json");
    }

    [Fact]
    public async Task GetStarships_ReturnsStarshipsList()
    {
        // Arrange - Seed database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var starships = new List<Starship>
        {
            new() { Name = "X-wing", Model = "T-65 X-wing", Manufacturer = "Incom Corporation" },
            new() { Name = "TIE Fighter", Model = "Twin Ion Engine", Manufacturer = "Sienar Fleet Systems" }
        };
        context.Starships.AddRange(starships);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/starships");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var returnedStarships = JsonConvert.DeserializeObject<List<Starship>>(content);
        returnedStarships.Should().HaveCount(2);
        returnedStarships!.Should().Contain(s => s.Name == "X-wing");
        returnedStarships.Should().Contain(s => s.Name == "TIE Fighter");
    }

    [Fact]
    public async Task GetStarship_WithValidId_ReturnsStarship()
    {
        // Arrange - Seed database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var starship = new Starship 
        { 
            Name = "Millennium Falcon", 
            Model = "YT-1300 light freighter", 
            Manufacturer = "Corellian Engineering Corporation" 
        };
        context.Starships.Add(starship);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/starships/{starship.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var returnedStarship = JsonConvert.DeserializeObject<Starship>(content);
        returnedStarship!.Name.Should().Be("Millennium Falcon");
        returnedStarship.Id.Should().Be(starship.Id);
    }

    [Fact]
    public async Task GetStarship_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/starships/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateStarship_WithValidData_ReturnsCreatedStarship()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var createDto = new CreateStarshipDto
        {
            Name = "A-wing",
            Model = "RZ-1 A-wing interceptor",
            Manufacturer = "Alliance Underground Engineering"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/starships", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadAsStringAsync();
        var createdStarship = JsonConvert.DeserializeObject<Starship>(content);
        createdStarship!.Name.Should().Be(createDto.Name);
        createdStarship.Model.Should().Be(createDto.Model);
        createdStarship.Id.Should().BeGreaterThan(0);

        // Verify it was actually saved to database
        var savedStarship = await context.Starships.FindAsync(createdStarship.Id);
        savedStarship.Should().NotBeNull();
        savedStarship!.Name.Should().Be(createDto.Name);
    }

    [Fact]
    public async Task UpdateStarship_WithValidData_ReturnsUpdatedStarship()
    {
        // Arrange - Seed database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var starship = new Starship 
        { 
            Name = "B-wing", 
            Model = "A/SF-01 B-wing starfighter",
            Manufacturer = "Slayn & Korpil"
        };
        context.Starships.Add(starship);
        await context.SaveChangesAsync();

        var updateDto = new UpdateStarshipDto
        {
            Name = "Modified B-wing",
            Model = "A/SF-01 Modified B-wing"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/starships/{starship.Id}", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var updatedStarship = JsonConvert.DeserializeObject<Starship>(content);
        updatedStarship!.Name.Should().Be(updateDto.Name);
        updatedStarship.Model.Should().Be(updateDto.Model);
        updatedStarship.Id.Should().Be(starship.Id);

        // Verify it was actually updated in database
        var savedStarship = await context.Starships.FindAsync(starship.Id);
        savedStarship!.Name.Should().Be(updateDto.Name);
    }

    [Fact]
    public async Task DeleteStarship_WithValidId_ReturnsNoContent()
    {
        // Arrange - Seed database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var starship = new Starship 
        { 
            Name = "TIE Interceptor", 
            Model = "TIE/IN interceptor",
            Manufacturer = "Sienar Fleet Systems"
        };
        context.Starships.Add(starship);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/starships/{starship.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it was actually deleted from database
        var deletedStarship = await context.Starships.FindAsync(starship.Id);
        deletedStarship.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStarship_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/starships/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
