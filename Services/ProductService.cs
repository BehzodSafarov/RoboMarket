using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoboMarketPro.Components.Pages;
using RoboMarketPro.Data;
using RoboMarketPro.Models;

namespace RoboMarketPro.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(string category, string name)
    {
        try
        {
            _logger.LogInformation("Retrieving all products");

            // Start with a base query, using AsNoTracking for read-only queries
            var query = _context.Products
                                .AsNoTracking()
                                .Include(p => p.Category)
                                .AsQueryable();

            // Apply category filter if provided
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category.Name == category);
            }

            // Apply name filter if provided
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.Name.Contains(name));
            }

            // Execute the query and get the list of products
            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            throw;
        }
    }


    public async Task<Product> GetProductByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Retrieving product with ID {id}");
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {id} not found");
                throw new Exception("Product not found");
            }
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving product with ID {id}");
            throw;
        }
    }

    public async Task AddProductAsync(Product product)
    {
        try
        {
            _logger.LogInformation("Adding a new product");
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Name == product.Name);
            if (existingProduct != null)
            {
                _logger.LogWarning($"Product with name {product.Name} already exists");
                throw new Exception("Product with the same name already exists");
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Product {product.Name} added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product");
            throw;
        }
    }

    public async Task UpdateProductAsync(Product product)
    {
        try
        {
            _logger.LogInformation($"Updating product with ID {product.Id}");
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null)
            {
                _logger.LogWarning($"Product with ID {product.Id} not found");
                throw new Exception("Product not found");
            }

            _context.Entry(existingProduct).CurrentValues.SetValues(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Product with ID {product.Id} updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            throw;
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            _logger.LogInformation($"Deleting product with ID {id}");
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Product with ID {id} deleted successfully");
            }
            else
            {
                _logger.LogWarning($"Product with ID {id} not found");
                throw new Exception("Product not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting product with ID {id}");
            throw;
        }
    }
}
