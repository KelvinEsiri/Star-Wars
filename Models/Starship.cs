using System.ComponentModel.DataAnnotations;

namespace Star_Wars.Models
{
    public class Starship
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string? Model { get; set; }
        
        [MaxLength(200)]
        public string? Manufacturer { get; set; }
        
        [MaxLength(20)]
        public string? CostInCredits { get; set; }
        
        [MaxLength(20)]
        public string? Length { get; set; }
        
        [MaxLength(20)]
        public string? MaxAtmospheringSpeed { get; set; }
        
        [MaxLength(10)]
        public int? Crew { get; set; }
        
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
        
        public DateTime Created { get; set; }
        
        public DateTime Edited { get; set; }
        
        [MaxLength(500)]
        public string? Url { get; set; }
        
        // Additional properties for local management
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
    }
}
