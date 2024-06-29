using Microsoft.EntityFrameworkCore;
using RoboMarketPro.Data;
using RoboMarketPro.Models;

namespace RoboMarketPro.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all categories");
            return await _context.Categories.Include(c => c.Products).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            throw;
        }
    }

    public async Task<Category> GetCategoryByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Retrieving category with ID {id}");
            var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                _logger.LogWarning($"Category with ID {id} not found");
                throw new Exception("Category not found");
            }
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving category with ID {id}");
            throw;
        }
    }

    public async Task AddCategoryAsync(Category category)
    {
        try
        {
            _logger.LogInformation("Adding a new category");
            var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name);
            if (existingCategory != null)
            {
                _logger.LogWarning($"Category with name {category.Name} already exists");
                throw new Exception("Category with the same name already exists");
            }

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Category {category.Name} added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding category");
            throw;
        }
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        try
        {
            _logger.LogInformation($"Updating category with ID {category.Id}");
            var existingCategory = await _context.Categories.FindAsync(category.Id);
            if (existingCategory == null)
            {
                _logger.LogWarning($"Category with ID {category.Id} not found");
                throw new Exception("Category not found");
            }

            _context.Entry(existingCategory).CurrentValues.SetValues(category);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Category with ID {category.Id} updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            throw;
        }
    }

    public async Task DeleteCategoryAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Deleting category with ID {id}");
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Category with ID {id} deleted successfully");
            }
            else
            {
                _logger.LogWarning($"Category with ID {id} not found");
                throw new Exception("Category not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting category with ID {id}");
            throw;
        }
    }
}
