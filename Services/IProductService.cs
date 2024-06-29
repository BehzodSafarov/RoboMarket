using RoboMarketPro.Models;

namespace RoboMarketPro.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync(string category, string name);
    Task<Product> GetProductByIdAsync(int id);
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}
