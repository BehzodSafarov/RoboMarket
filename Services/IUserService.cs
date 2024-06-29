using RoboMarketPro.Models;

namespace RoboMarketPro.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User> GetUserByIdAsync(int id);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
    Task<bool> LoginAsync(string phoneNumber, string password);
    Task<bool> RegisterAsync(string phoneNumber, string password);
    Task LogoutAsync();
}

