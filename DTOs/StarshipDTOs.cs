using System.ComponentModel.DataAnnotations;

namespace Star_Wars.DTOs
{
    public class StarshipDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Manufacturer { get; set; }
        public string? CostInCredits { get; set; }
        public string? Length { get; set; }
        public string? MaxAtmospheringSpeed { get; set; }
        public string? Crew { get; set; }
        public string? Passengers { get; set; }
        public string? CargoCapacity { get; set; }
        public string? Consumables { get; set; }
        public string? HyperdriveRating { get; set; }
        public string? Mglt { get; set; }
        public string? StarshipClass { get; set; }
        public List<string> Pilots { get; set; } = new();
        public List<string> Films { get; set; } = new();
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public string? Url { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateStarshipDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? Model { get; set; }
        
        [MaxLength(50)]
        public string? Manufacturer { get; set; }
        
        [MaxLength(20)]
        public string? CostInCredits { get; set; }
        
        [MaxLength(20)]
        public string? Length { get; set; }
        
        [MaxLength(20)]
        public string? MaxAtmospheringSpeed { get; set; }
        
        [MaxLength(10)]
        public string? Crew { get; set; }
        
        [MaxLength(20)]
        public string? Passengers { get; set; }
        
        [MaxLength(50)]
        public string? CargoCapacity { get; set; }
        
        [MaxLength(50)]
        public string? Consumables { get; set; }
        
        [MaxLength(20)]
        public string? HyperdriveRating { get; set; }
        
        [MaxLength(10)]
        public string? Mglt { get; set; }
        
        [MaxLength(50)]
        public string? StarshipClass { get; set; }
        
        public List<string> Pilots { get; set; } = new();
        
        public List<string> Films { get; set; } = new();
    }

    public class UpdateStarshipDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? Model { get; set; }
        
        [MaxLength(50)]
        public string? Manufacturer { get; set; }
        
        [MaxLength(20)]
        public string? CostInCredits { get; set; }
        
        [MaxLength(20)]
        public string? Length { get; set; }
        
        [MaxLength(20)]
        public string? MaxAtmospheringSpeed { get; set; }
        
        [MaxLength(10)]
        public string? Crew { get; set; }
        
        [MaxLength(20)]
        public string? Passengers { get; set; }
        
        [MaxLength(50)]
        public string? CargoCapacity { get; set; }
        
        [MaxLength(50)]
        public string? Consumables { get; set; }
        
        [MaxLength(20)]
        public string? HyperdriveRating { get; set; }
        
        [MaxLength(10)]
        public string? Mglt { get; set; }
        
        [MaxLength(50)]
        public string? StarshipClass { get; set; }
        
        public List<string> Pilots { get; set; } = new();
        
        public List<string> Films { get; set; } = new();
    }

    public class StarshipQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; } = false;
        public string? Manufacturer { get; set; }
        public string? StarshipClass { get; set; }
        public bool? IsActive { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}
