# Star Wars API - Complete Project Creation & Execution Guide

A comprehensive step-by-step guide to create and execute the Star Wars API project from scratch.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Project Setup](#project-setup)
- [Database Configuration](#database-configuration)
- [Core Implementation](#core-implementation)
- [Authentication System](#authentication-system)
- [API Development](#api-development)
- [Testing Framework](#testing-framework)
- [Docker Configuration](#docker-configuration)
- [Documentation](#documentation)
- [Execution & Deployment](#execution--deployment)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software
1. **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **Visual Studio 2022** or **VS Code** with C# extension
3. **SQL Server** - LocalDB (included with Visual Studio) or full instance
4. **Docker Desktop** - For containerization
5. **Git** - Version control
6. **Postman** or similar API testing tool (optional)

### Verify Installation
```bash
dotnet --version          # Should show 8.x.x
docker --version          # Verify Docker is installed
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"  # Test SQL Server
```

---

## Project Setup

### Step 1: Create Solution and Project
```bash
# Create project directory
mkdir "Star Wars API"
cd "Star Wars API"

# Create solution
dotnet new sln -n "Star Wars"

# Create Web API project
dotnet new webapi -n "Star Wars" --framework net8.0

# Add project to solution
dotnet sln add "Star Wars/Star Wars.csproj"
```

### Step 2: Install Required NuGet Packages
```bash
cd "Star Wars"

# Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design

# Identity Framework
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# AutoMapper
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# Validation
dotnet add package FluentValidation.AspNetCore

# Logging
dotnet add package NLog.Web.AspNetCore

# HTTP Client
dotnet add package Microsoft.Extensions.Http

# Testing Packages
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq
dotnet add package FluentAssertions
```

---

## Database Configuration

### Step 3: Create Data Models

**Create `Models/ApplicationUser.cs`:**
```csharp
using Microsoft.AspNetCore.Identity;

namespace Star_Wars.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public DateTime ApiKeyCreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

**Create `Models/Starship.cs`:**
```csharp
namespace Star_Wars.Models
{
    public class Starship
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string CostInCredits { get; set; } = string.Empty;
        public string Length { get; set; } = string.Empty;
        public string MaxAtmospheringSpeed { get; set; } = string.Empty;
        public string Crew { get; set; } = string.Empty;
        public string Passengers { get; set; } = string.Empty;
        public string CargoCapacity { get; set; } = string.Empty;
        public string Consumables { get; set; } = string.Empty;
        public string HyperdriveRating { get; set; } = string.Empty;
        public string MGLT { get; set; } = string.Empty;
        public string StarshipClass { get; set; } = string.Empty;
        public List<string> Pilots { get; set; } = new List<string>();
        public List<string> Films { get; set; } = new List<string>();
        public string Url { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

### Step 4: Create Database Context

**Create `Data/ApplicationDbContext.cs`:**
```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Star_Wars.Models;
using System.Text.Json;

namespace Star_Wars.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Starship> Starships { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Starship entity
            builder.Entity<Starship>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Manufacturer);
                entity.HasIndex(e => e.StarshipClass);
                entity.HasIndex(e => e.IsActive);

                // JSON configuration for list properties
                entity.Property(e => e.Pilots)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                        v => JsonSerializer.Deserialize<List<string>>(v, default(JsonSerializerOptions)) ?? new List<string>());

                entity.Property(e => e.Films)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                        v => JsonSerializer.Deserialize<List<string>>(v, default(JsonSerializerOptions)) ?? new List<string>());
            });

            // Configure ApplicationUser entity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ApiKey).IsUnique();
                
                entity.Property(e => e.ApiKey)
                    .HasMaxLength(255);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Starship && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is Starship starship)
                {
                    starship.UpdatedAt = DateTime.UtcNow;

                    if (entityEntry.State == EntityState.Added)
                    {
                        starship.CreatedAt = DateTime.UtcNow;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
```

### Step 5: Configure Connection String

**Update `appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StarWarsDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "ApiKeySettings": {
    "ApiKey": "SW-API-KEY-2024-SECURE-ACCESS-TOKEN-12345",
    "HeaderName": "X-API-Key"
  },
  "AdminSettings": {
    "Order66Key": "FOR-PADME-FOR-LOVE",
    "RequireConfirmation": true,
    "AuditAllOperations": true
  },
  "SwapiSettings": {
    "BaseUrl": "https://swapi.info/api/starships"
  },
  "DatabaseSeeding": {
    "EnableAutoSeed": true,
    "SeedOnStartup": true,
    "ForceReseed": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 6: Create Database Migration
```bash
# Add initial migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

---

## Core Implementation

### Step 7: Create Repository Pattern

**Create `Repositories/Interfaces/IStarshipRepository.cs`:**
```csharp
using Star_Wars.DTOs;
using Star_Wars.Models;

namespace Star_Wars.Repositories.Interfaces
{
    public interface IStarshipRepository
    {
        Task<PagedResult<Starship>> GetAllAsync(StarshipQueryDto query);
        Task<Starship?> GetByIdAsync(int id);
        Task<Starship> CreateAsync(Starship starship);
        Task<Starship> UpdateAsync(Starship starship);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string name);
        Task<List<string>> GetManufacturersAsync();
        Task<List<string>> GetClassesAsync();
        Task BulkInsertAsync(List<Starship> starships);
    }
}
```

**Create `Repositories/Implementations/StarshipRepository.cs`:**
```csharp
using Microsoft.EntityFrameworkCore;
using Star_Wars.Data;
using Star_Wars.DTOs;
using Star_Wars.Models;
using Star_Wars.Repositories.Interfaces;

namespace Star_Wars.Repositories.Implementations
{
    public class StarshipRepository : IStarshipRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StarshipRepository> _logger;

        public StarshipRepository(ApplicationDbContext context, ILogger<StarshipRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<Starship>> GetAllAsync(StarshipQueryDto query)
        {
            var queryable = _context.Starships.Where(s => s.IsActive);

            // Apply filters
            if (!string.IsNullOrEmpty(query.Name))
            {
                queryable = queryable.Where(s => s.Name.Contains(query.Name));
            }

            if (!string.IsNullOrEmpty(query.Manufacturer))
            {
                queryable = queryable.Where(s => s.Manufacturer.Contains(query.Manufacturer));
            }

            if (!string.IsNullOrEmpty(query.StarshipClass))
            {
                queryable = queryable.Where(s => s.StarshipClass.Contains(query.StarshipClass));
            }

            // Apply sorting
            queryable = query.SortBy?.ToLower() switch
            {
                "name" => query.SortOrder == "desc" ? queryable.OrderByDescending(s => s.Name) : queryable.OrderBy(s => s.Name),
                "manufacturer" => query.SortOrder == "desc" ? queryable.OrderByDescending(s => s.Manufacturer) : queryable.OrderBy(s => s.Manufacturer),
                "starshipclass" => query.SortOrder == "desc" ? queryable.OrderByDescending(s => s.StarshipClass) : queryable.OrderBy(s => s.StarshipClass),
                "createdat" => query.SortOrder == "desc" ? queryable.OrderByDescending(s => s.CreatedAt) : queryable.OrderBy(s => s.CreatedAt),
                _ => queryable.OrderBy(s => s.Name)
            };

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Starship>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
            };
        }

        public async Task<Starship?> GetByIdAsync(int id)
        {
            return await _context.Starships
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Starship> CreateAsync(Starship starship)
        {
            _context.Starships.Add(starship);
            await _context.SaveChangesAsync();
            return starship;
        }

        public async Task<Starship> UpdateAsync(Starship starship)
        {
            _context.Starships.Update(starship);
            await _context.SaveChangesAsync();
            return starship;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var starship = await GetByIdAsync(id);
            if (starship == null) return false;

            starship.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Starships
                .AnyAsync(s => s.Name == name && s.IsActive);
        }

        public async Task<List<string>> GetManufacturersAsync()
        {
            return await _context.Starships
                .Where(s => s.IsActive)
                .Select(s => s.Manufacturer)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<List<string>> GetClassesAsync()
        {
            return await _context.Starships
                .Where(s => s.IsActive)
                .Select(s => s.StarshipClass)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task BulkInsertAsync(List<Starship> starships)
        {
            _context.Starships.AddRange(starships);
            await _context.SaveChangesAsync();
        }
    }
}
```

### Step 8: Create DTOs

**Create `DTOs/StarshipDto.cs`:**
```csharp
namespace Star_Wars.DTOs
{
    public class StarshipDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string CostInCredits { get; set; } = string.Empty;
        public string Length { get; set; } = string.Empty;
        public string MaxAtmospheringSpeed { get; set; } = string.Empty;
        public string Crew { get; set; } = string.Empty;
        public string Passengers { get; set; } = string.Empty;
        public string CargoCapacity { get; set; } = string.Empty;
        public string Consumables { get; set; } = string.Empty;
        public string HyperdriveRating { get; set; } = string.Empty;
        public string MGLT { get; set; } = string.Empty;
        public string StarshipClass { get; set; } = string.Empty;
        public List<string> Pilots { get; set; } = new List<string>();
        public List<string> Films { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
```

**Create `DTOs/PagedResult.cs`:**
```csharp
namespace Star_Wars.DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}
```

**Create `DTOs/StarshipQueryDto.cs`:**
```csharp
namespace Star_Wars.DTOs
{
    public class StarshipQueryDto
    {
        public string? Name { get; set; }
        public string? Manufacturer { get; set; }
        public string? StarshipClass { get; set; }
        public string? SortBy { get; set; } = "name";
        public string? SortOrder { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
```

---

## Authentication System

### Step 9: Create API Key Middleware

**Create `Middleware/ApiKeyMiddleware.cs`:**
```csharp
using Microsoft.AspNetCore.Identity;
using Star_Wars.Models;

namespace Star_Wars.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
        {
            // Skip authentication for certain endpoints
            var path = context.Request.Path.ToString();
            if (ShouldSkipAuthentication(path))
            {
                await _next(context);
                return;
            }

            var apiKey = GetApiKeyFromRequest(context);
            
            if (string.IsNullOrEmpty(apiKey))
            {
                await WriteUnauthorizedResponse(context, "API key is required");
                return;
            }

            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.ApiKey == apiKey && u.IsActive);

            if (user == null)
            {
                await WriteUnauthorizedResponse(context, "Invalid API key");
                return;
            }

            // Set user context
            context.Items["CurrentUser"] = user;
            context.Items["UserId"] = user.Id;

            await _next(context);
        }

        private static bool ShouldSkipAuthentication(string path)
        {
            var publicPaths = new[]
            {
                "/api/auth/register",
                "/api/auth/login",
                "/swagger",
                "/health",
                "/demo.html",
                "/favicon.ico"
            };

            return publicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        private static string? GetApiKeyFromRequest(HttpContext context)
        {
            // Try header first
            if (context.Request.Headers.TryGetValue("X-API-Key", out var headerValue))
            {
                return headerValue.FirstOrDefault();
            }

            // Try cookie
            if (context.Request.Cookies.TryGetValue("X-API-Key", out var cookieValue))
            {
                return cookieValue;
            }

            return null;
        }

        private static async Task WriteUnauthorizedResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(message);
        }
    }
}
```

### Step 10: Create Authentication Services

**Create `Services/Interfaces/IAuthService.cs`:**
```csharp
using Star_Wars.DTOs;

namespace Star_Wars.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<bool> ForgotPasswordAsync(string email);
        Task<UserProfileDto> GetUserProfileAsync(string userId);
    }
}
```

---

## API Development

### Step 11: Create Controllers

**Create `Controllers/StarshipsController.cs`:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Star_Wars.DTOs;
using Star_Wars.Services.Interfaces;

namespace Star_Wars.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StarshipsController : ControllerBase
    {
        private readonly IStarshipService _starshipService;
        private readonly ILogger<StarshipsController> _logger;

        public StarshipsController(IStarshipService starshipService, ILogger<StarshipsController> logger)
        {
            _starshipService = starshipService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<StarshipDto>>> GetStarships([FromQuery] StarshipQueryDto query)
        {
            try
            {
                var result = await _starshipService.GetAllAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving starships");
                return StatusCode(500, "An error occurred while retrieving starships");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StarshipDto>> GetStarship(int id)
        {
            try
            {
                var starship = await _starshipService.GetByIdAsync(id);
                if (starship == null)
                {
                    return NotFound($"Starship with ID {id} not found");
                }
                return Ok(starship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving starship with ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the starship");
            }
        }

        [HttpPost]
        public async Task<ActionResult<StarshipDto>> CreateStarship(CreateStarshipDto createStarshipDto)
        {
            try
            {
                var starship = await _starshipService.CreateAsync(createStarshipDto);
                return CreatedAtAction(nameof(GetStarship), new { id = starship.Id }, starship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating starship");
                return StatusCode(500, "An error occurred while creating the starship");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<StarshipDto>> UpdateStarship(int id, UpdateStarshipDto updateStarshipDto)
        {
            try
            {
                var starship = await _starshipService.UpdateAsync(id, updateStarshipDto);
                if (starship == null)
                {
                    return NotFound($"Starship with ID {id} not found");
                }
                return Ok(starship);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating starship with ID {Id}", id);
                return StatusCode(500, "An error occurred while updating the starship");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStarship(int id)
        {
            try
            {
                var result = await _starshipService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Starship with ID {id} not found");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting starship with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the starship");
            }
        }

        [HttpGet("manufacturers")]
        public async Task<ActionResult<List<string>>> GetManufacturers()
        {
            try
            {
                var manufacturers = await _starshipService.GetManufacturersAsync();
                return Ok(manufacturers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manufacturers");
                return StatusCode(500, "An error occurred while retrieving manufacturers");
            }
        }

        [HttpGet("classes")]
        public async Task<ActionResult<List<string>>> GetClasses()
        {
            try
            {
                var classes = await _starshipService.GetClassesAsync();
                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving starship classes");
                return StatusCode(500, "An error occurred while retrieving starship classes");
            }
        }
    }
}
```

### Step 12: Create SWAPI Integration

**Create `Services/Interfaces/ISwapiService.cs`:**
```csharp
using Star_Wars.Models;

namespace Star_Wars.Services.Interfaces
{
    public interface ISwapiService
    {
        Task<List<SwapiStarship>> FetchAllStarshipsAsync();
        Task SeedStarshipsAsync();
    }
}
```

**Create `Services/Implementations/SwapiService.cs`:**
```csharp
using AutoMapper;
using Star_Wars.Models;
using Star_Wars.Repositories.Interfaces;
using Star_Wars.Services.Interfaces;
using System.Text.Json;

namespace Star_Wars.Services.Implementations
{
    public class SwapiService : ISwapiService
    {
        private readonly HttpClient _httpClient;
        private readonly IStarshipRepository _starshipRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SwapiService> _logger;
        private readonly string _baseUrl;

        public SwapiService(
            HttpClient httpClient,
            IStarshipRepository starshipRepository,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<SwapiService> logger)
        {
            _httpClient = httpClient;
            _starshipRepository = starshipRepository;
            _mapper = mapper;
            _logger = logger;
            _baseUrl = configuration["SwapiSettings:BaseUrl"] ?? "https://swapi.info/api/starships";
        }

        public async Task<List<SwapiStarship>> FetchAllStarshipsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching starships from SWAPI");
                
                var response = await _httpClient.GetAsync(_baseUrl);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var starships = JsonSerializer.Deserialize<List<SwapiStarship>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully fetched {Count} starships from SWAPI", starships?.Count ?? 0);
                return starships ?? new List<SwapiStarship>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching starships from SWAPI");
                throw;
            }
        }

        public async Task SeedStarshipsAsync()
        {
            try
            {
                _logger.LogInformation("Starting starship seeding process");

                var swapiStarships = await FetchAllStarshipsAsync();
                var newStarships = new List<Starship>();

                foreach (var swapiStarship in swapiStarships)
                {
                    if (!await _starshipRepository.ExistsAsync(swapiStarship.Name))
                    {
                        var starship = _mapper.Map<Starship>(swapiStarship);
                        newStarships.Add(starship);
                    }
                }

                if (newStarships.Any())
                {
                    await _starshipRepository.BulkInsertAsync(newStarships);
                    _logger.LogInformation("Successfully seeded {Count} new starships", newStarships.Count);
                }
                else
                {
                    _logger.LogInformation("No new starships to seed - all starships already exist in database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding starships");
                throw;
            }
        }
    }
}
```

---

## Testing Framework

### Step 13: Create Test Project
```bash
# Create test project
dotnet new xunit -n "Star Wars.Tests"

# Add test project packages
cd "Star Wars.Tests"
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Moq
dotnet add package FluentAssertions

# Add reference to main project
dotnet add reference "../Star Wars/Star Wars.csproj"

# Add test project to solution
cd ..
dotnet sln add "Star Wars.Tests/Star Wars.Tests.csproj"
```

### Step 14: Create Integration Tests

**Create `Tests/Integration/StarshipsControllerTests.cs`:**
```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Star_Wars.Data;
using Star_Wars.DTOs;
using Star_Wars.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Star_Wars.Tests.Integration
{
    public class StarshipsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public StarshipsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetStarships_ReturnsSuccessStatusCode()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/starships");

            // Assert
            response.Should().BeSuccessful();
        }

        [Fact]
        public async Task CreateStarship_WithValidData_ReturnsCreatedStarship()
        {
            // Arrange
            var newStarship = new CreateStarshipDto
            {
                Name = "Test Starship",
                Manufacturer = "Test Corp",
                Model = "Test Model"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/starships", newStarship);

            // Assert
            response.Should().BeSuccessful();
            var createdStarship = await response.Content.ReadFromJsonAsync<StarshipDto>();
            createdStarship.Should().NotBeNull();
            createdStarship!.Name.Should().Be(newStarship.Name);
        }
    }
}
```

---

## Docker Configuration

### Step 15: Create Dockerfile

**Create `Dockerfile`:**
```dockerfile
# Use the official .NET 8 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Star Wars.csproj", "."]
RUN dotnet restore "./Star Wars.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Star Wars.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Star Wars.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "Star Wars.dll"]
```

### Step 16: Create Docker Compose

**Create `docker-compose.yml`:**
```yaml
services:
  starwars-api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=StarWarsDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Connection Timeout=30;Encrypt=False
    depends_on:
      sqlserver:
        condition: service_healthy
    networks:
      - starwars-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - SA_PASSWORD=YourStrong!Passw0rd
      - ACCEPT_EULA=Y
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - starwars-network
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q 'SELECT 1' -C || exit 1"]
      interval: 10s
      timeout: 3s
      retries: 10
      start_period: 10s

volumes:
  sqlserver_data:

networks:
  starwars-network:
    driver: bridge
```

### Step 17: Create .dockerignore

**Create `.dockerignore`:**
```
bin/
obj/
*.user
*.suo
*.cache
logs/
.vs/
.vscode/
*.log
README.md
.git/
.gitignore
Dockerfile*
docker-compose*
```

---

## Documentation

### Step 18: Create API Documentation

**Create `docs/API_REFERENCE.md`:**
```markdown
# Star Wars API Reference

## Authentication
All endpoints require authentication via:
- **Header**: `X-API-Key: your-api-key`
- **Cookie**: `X-API-Key=your-api-key`

## Endpoints

### GET /api/starships
Get paginated list of starships with filtering and sorting.

**Query Parameters:**
- `name` (string): Filter by starship name
- `manufacturer` (string): Filter by manufacturer
- `starshipClass` (string): Filter by starship class
- `sortBy` (string): Sort field (name, manufacturer, starshipClass, createdAt)
- `sortOrder` (string): Sort direction (asc, desc)
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)

**Response:**
```json
{
  "items": [...],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true
}
```

### POST /api/starships
Create a new starship.

**Request Body:**
```json
{
  "name": "Millennium Falcon",
  "manufacturer": "Corellian Engineering Corporation",
  "model": "YT-1300 light freighter"
}
```

## Error Responses
- `400` - Bad Request
- `401` - Unauthorized (Invalid API key)
- `404` - Not Found
- `500` - Internal Server Error
```

### Step 19: Create Development Guide

**Create `docs/DEVELOPMENT.md`:**
```markdown
# Development Guide

## Architecture
The Star Wars API follows Clean Architecture principles:

- **Controllers**: Handle HTTP requests/responses
- **Services**: Business logic implementation
- **Repositories**: Data access layer
- **Models**: Domain entities
- **DTOs**: Data transfer objects

## Development Workflow
1. Create feature branch
2. Implement changes with tests
3. Run all tests: `dotnet test`
4. Create pull request
5. Code review and merge

## Testing
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "TestMethodName"
```

## Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```
```

---

## Execution & Deployment

### Step 20: Configure Program.cs

**Update `Program.cs`:**
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using AutoMapper;
using Star_Wars.Configuration;
using Star_Wars.Data;
using Star_Wars.Models;
using Star_Wars.Repositories.Implementations;
using Star_Wars.Repositories.Interfaces;
using Star_Wars.Services.Implementations;
using Star_Wars.Services.Interfaces;
using Star_Wars.Middleware;
using Star_Wars.Extensions;
using FluentValidation.AspNetCore;

// Early init of NLog
var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // NLog setup
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Controllers and API configuration
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    
    // Configure options
    builder.Services.Configure<DatabaseSeedingOptions>(
        builder.Configuration.GetSection(DatabaseSeedingOptions.SectionName));
    
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Star Wars API", Version = "v1" });
        
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "API Key: X-API-Key",
            In = ParameterLocation.Header,
            Name = "X-API-Key",
            Type = SecuritySchemeType.ApiKey
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
                },
                Array.Empty<string>()
            }
        });
    });

    // Register services
    builder.Services.AddHttpClient();
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddFluentValidationAutoValidation();
    
    // Repository and service registration
    builder.Services.AddScoped<IStarshipRepository, StarshipRepository>();
    builder.Services.AddScoped<IStarshipService, StarshipService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
    builder.Services.AddScoped<ISwapiService, SwapiService>();
    builder.Services.AddScoped<IDatabaseSeederService, DatabaseSeederService>();

    var app = builder.Build();

    // Configure pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseMiddleware<ApiKeyMiddleware>();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Initialize and seed database
    await app.InitializeDatabaseAsync();

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}

public partial class Program { }
```

### Step 21: Local Development Execution

```bash
# Restore packages
dotnet restore

# Run database migrations
dotnet ef database update

# Run the application
dotnet run

# Access endpoints:
# - API: https://localhost:7108
# - Swagger: https://localhost:7108/swagger
# - Demo: https://localhost:7108/demo.html
```

### Step 22: Docker Execution

```bash
# Build and run with Docker Compose
docker-compose up --build -d

# Access endpoints:
# - API: http://localhost:8080
# - Demo: http://localhost:8080/demo.html

# View logs
docker-compose logs -f starwars-api

# Stop containers
docker-compose down
```

### Step 23: Production Deployment

```bash
# Build production image
docker build -t starwars-api:latest .

# Run production container
docker run -d \
  --name starwars-api \
  -p 80:80 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="your-production-connection-string" \
  starwars-api:latest
```

---

## Troubleshooting

### Common Issues & Solutions

**Issue: Database Connection Failed**
```
Solution: Verify SQL Server is running and connection string is correct
Command: sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT @@VERSION"
```

**Issue: API Key Authentication Failed**
```
Solution: Register a user first to get an API key
Endpoint: POST /api/auth/register
```

**Issue: Docker Build Failed**
```
Solution: Check .dockerignore and ensure all dependencies are restored
Command: docker build --no-cache -t starwars-api .
```

**Issue: SWAPI Seeding Failed**
```
Solution: Check internet connectivity and SWAPI endpoint availability
URL: https://swapi.info/api/starships
```

**Issue: Port Already in Use**
```
Solution: Change ports in docker-compose.yml or kill existing processes
Command: netstat -an | findstr :8080
```

### Development Tips

1. **Enable Hot Reload**: Use `dotnet watch run` for development
2. **Database Reset**: Delete database and run `dotnet ef database update`
3. **Clear Docker Cache**: Use `docker system prune -a`
4. **View Logs**: Check `logs/` directory for application logs
5. **Test API**: Use Postman, curl, or the included demo.html page

---

## Project Structure Summary

```
📁 Star Wars API/
├── 📁 Configuration/         # App configuration classes
├── 📁 Controllers/          # API controllers
├── 📁 Data/                 # Database context
├── 📁 docs/                 # Documentation
├── 📁 DTOs/                 # Data transfer objects
├── 📁 Extensions/           # Extension methods
├── 📁 Mapping/              # AutoMapper profiles
├── 📁 Middleware/           # Custom middleware
├── 📁 Migrations/           # EF Core migrations
├── 📁 Models/               # Domain models
├── 📁 Properties/           # Project properties
├── 📁 Repositories/         # Data access layer
├── 📁 Services/             # Business logic
├── 📁 Tests/                # Test projects
├── 📁 Validators/           # Input validation
├── 📁 wwwroot/              # Static files
├── 📄 Program.cs            # Application entry point
├── 📄 Dockerfile           # Container definition
├── 📄 docker-compose.yml   # Multi-container setup
└── 📄 README.md             # Project documentation
```

---

## 🎉 Congratulations!

You now have a fully functional Star Wars API with:
- ✅ Complete CRUD operations for starships
- ✅ Secure API key authentication
- ✅ Automatic database seeding from SWAPI
- ✅ Docker containerization
- ✅ Comprehensive testing
- ✅ Interactive documentation
- ✅ Production-ready deployment

**May the Force be with you!** ⭐
