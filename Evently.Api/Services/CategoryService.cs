using Evently.Api.Data;
using Evently.Api.DTOs.Categories;
using Evently.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly EventlyDbContext _context;

        public CategoryService(EventlyDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _context.EventCategories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            return await _context.EventCategories
                .AsNoTracking()
                .Where(c => c.Id == id && c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsActive = c.IsActive
                })
                .FirstOrDefaultAsync();
        }
    }
}
