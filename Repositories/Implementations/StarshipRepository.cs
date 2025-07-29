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

        public StarshipRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Starship>> GetAllAsync(StarshipQueryDto query)
        {
            var queryable = _context.Starships.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.Search))
            {
                queryable = queryable.Where(s => 
                    s.Name.Contains(query.Search) ||
                    (s.Model != null && s.Model.Contains(query.Search)) ||
                    (s.Manufacturer != null && s.Manufacturer.Contains(query.Search)) ||
                    (s.StarshipClass != null && s.StarshipClass.Contains(query.Search)));
            }

            if (!string.IsNullOrEmpty(query.Manufacturer))
            {
                queryable = queryable.Where(s => s.Manufacturer == query.Manufacturer);
            }

            if (!string.IsNullOrEmpty(query.StarshipClass))
            {
                queryable = queryable.Where(s => s.StarshipClass == query.StarshipClass);
            }

            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(s => s.IsActive == query.IsActive.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(query.SortBy))
            {
                queryable = query.SortBy.ToLower() switch
                {
                    "name" => query.SortDescending ? queryable.OrderByDescending(s => s.Name) : queryable.OrderBy(s => s.Name),
                    "model" => query.SortDescending ? queryable.OrderByDescending(s => s.Model) : queryable.OrderBy(s => s.Model),
                    "manufacturer" => query.SortDescending ? queryable.OrderByDescending(s => s.Manufacturer) : queryable.OrderBy(s => s.Manufacturer),
                    "starshipclass" => query.SortDescending ? queryable.OrderByDescending(s => s.StarshipClass) : queryable.OrderBy(s => s.StarshipClass),
                    "created" => query.SortDescending ? queryable.OrderByDescending(s => s.CreatedAt) : queryable.OrderBy(s => s.CreatedAt),
                    _ => queryable.OrderBy(s => s.Name)
                };
            }
            else
            {
                queryable = queryable.OrderBy(s => s.Name);
            }

            var totalCount = await queryable.CountAsync();
            var items = await queryable
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return new PagedResult<Starship>
            {
                Data = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = totalPages,
                HasNext = query.Page < totalPages,
                HasPrevious = query.Page > 1
            };
        }

        public async Task<Starship?> GetByIdAsync(int id)
        {
            return await _context.Starships.FindAsync(id);
        }

        public async Task<Starship> CreateAsync(Starship starship)
        {
            _context.Starships.Add(starship);
            await _context.SaveChangesAsync();
            return starship;
        }

        public async Task<Starship> UpdateAsync(Starship starship)
        {
            _context.Entry(starship).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return starship;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var starship = await _context.Starships.FindAsync(id);
            if (starship == null)
                return false;

            starship.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string name)
        {
            return await _context.Starships.AnyAsync(s => s.Name == name);
        }

        public async Task<int> BulkInsertAsync(List<Starship> starships)
        {
            _context.Starships.AddRange(starships);
            return await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetManufacturersAsync()
        {
            return await _context.Starships
                .Where(s => !string.IsNullOrEmpty(s.Manufacturer) && s.IsActive)
                .Select(s => s.Manufacturer!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<List<string>> GetStarshipClassesAsync()
        {
            return await _context.Starships
                .Where(s => !string.IsNullOrEmpty(s.StarshipClass) && s.IsActive)
                .Select(s => s.StarshipClass!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}
